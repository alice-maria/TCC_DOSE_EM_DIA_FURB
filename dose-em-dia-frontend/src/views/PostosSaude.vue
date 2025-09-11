<template>
  <v-container fluid class="pa-0">
    <div class="pagina-postos">
      <!-- Cabeçalho -->
      <div class="header">
        <h1 class="titulo" @click="$router.push('/home')">Dose em dia</h1>
        <div class="usuario">
          <UsuarioMenu />
        </div>
      </div>

      <!-- Breadcrumbs -->
      <v-breadcrumbs class="meus-breadcrumbs px-6" :items="breadcrumbs">
        <template #item="{ item }">
          <span :class="['breadcrumb-link', { 'breadcrumb-laranja': !item.to }]" @click="item.to && navegar(item.to)"
            style="cursor: pointer;">
            <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt="" class="breadcrumb-home-img" />
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <!-- Conteúdo -->
      <div class="conteudo">
        <div class="lista px-6 pb-8">
          <!-- Overlay de carregamento -->
          <v-overlay v-model="carregando" :persistent="true" class="d-flex align-center justify-center">
            <div class="loading-card">
              <v-progress-circular indeterminate size="36" width="4" />
              <div class="mt-3 text-center">
                <div class="fw-600">Buscando postos próximos…</div>
                <div class="text-caption text-medium-emphasis" v-if="progresso > 0 && progresso < 100">
                  {{ progresso }}%
                </div>
                <div class="text-caption text-medium-emphasis" v-else>
                  Isso pode levar alguns segundos.
                </div>
              </div>
            </div>
          </v-overlay>

          <v-card v-for="posto in postos" :key="posto.linkGoogleMaps || (posto.nome + posto.enderecoCompleto)"
            class="m3-card mb-3" variant="elevated" elevation="1" :ripple="true"
            @click="abrirMapa(posto.linkGoogleMaps)">
            <div class="card-conteudo">
              <div class="texto">
                <v-card-title class="m3-card__title">{{ posto.nome }}</v-card-title>
                <v-card-subtitle class="m3-card__subtitle">
                  {{ posto.enderecoCompleto }}
                </v-card-subtitle>
                <p class="distancia" v-if="posto.distancia">Aprox. {{ posto.distancia }}</p>
              </div>
              <a :href="posto.linkGoogleMaps" target="_blank" rel="noopener noreferrer" @click.stop>
                <img src="@/assets/icons/seta.svg" alt="Ir" class="seta" />
              </a>
            </div>
          </v-card>

          <p v-if="!carregando && postos.length === 0 && !erro" class="text-center mt-4 text-gray-600">
            Nenhum posto de vacinação encontrado.
          </p>
          <p v-if="erro" class="text-center mt-4" style="color:#dc2626">
            {{ erro }}
          </p>
        </div>
      </div>
    </div>
  </v-container>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import UsuarioMenu from '@/views/UsuarioMenu.vue'
import axios from 'axios'

const baseURL = (process.env.VUE_APP_API_BASE_URL || 'https://doseemdiabackend-production.up.railway.app').replace(/\/+$/, '')

const api = axios.create({
  baseURL,
  timeout: 20000,
})

const router = useRouter()

const nomeUsuario = ref('')
const breadcrumbs = ref([
  { text: 'Início', to: '/home', icon: 'mdi-home' },
  { text: 'Postos de saúde mais próximos' }
])

const postos = ref([])
const carregando = ref(false)
const erro = ref('')
const progresso = ref(0)
let timer = null
let controller = null 

function navegar(to) {
  router.push(to)
}

function abrirMapa(link) {
  if (!link) return
  window.open(link, '_blank', 'noopener,noreferrer')
}

function iniciarProgresso() {
  progresso.value = 0
  clearInterval(timer)
  timer = setInterval(() => {
    if (progresso.value < 90) progresso.value += 5
  }, 250)
}

function finalizarProgresso() {
  clearInterval(timer)
  progresso.value = 100
  setTimeout(() => (progresso.value = 0), 600)
}

function distanciaParaMetros(txt) {
  if (!txt || typeof txt !== 'string') return Number.POSITIVE_INFINITY
  const s = txt.trim().toLowerCase()
  if (s.endsWith('km')) {
    const n = parseFloat(s.replace('km', '').replace(/\./g, '').replace(',', '.').trim())
    return isNaN(n) ? Number.POSITIVE_INFINITY : n * 1000
  }
  if (s.endsWith('m')) {
    const n = parseInt(s.replace('m', '').trim(), 10)
    return isNaN(n) ? Number.POSITIVE_INFINITY : n
  }
  return Number.POSITIVE_INFINITY
}

function normalizarPosto(p) {
  return {
    nome: p?.nome ?? p?.Name ?? p?.name ?? 'Unidade de Saúde',
    enderecoCompleto: p?.enderecoCompleto ?? p?.Address ?? p?.address ?? '',
    distancia: p?.distancia ?? p?.DistanceText ?? p?.distanceText ?? null,
    linkGoogleMaps: p?.linkGoogleMaps ?? p?.GoogleMapsLink ?? p?.googleMapsLink ?? ''
  }
}

async function buscarPostosPorUsuario(usuarioId, jaRepetiu = false) {
  if (carregando.value) return
  carregando.value = true
  erro.value = ''
  iniciarProgresso()

  if (controller) controller.abort()
  controller = new AbortController()

  try {
    const resp = await api.get('/api/localizacao/proximos', {
      params: { usuarioId },
      validateStatus: () => true,
      timeout: 12000,
      signal: controller.signal
    })

    if (resp.status === 200) {
      const arr = Array.isArray(resp.data) ? resp.data : (Array.isArray(resp.data?.postos) ? resp.data.postos : [])
      const normalizados = arr.map(normalizarPosto)

      normalizados.sort((a, b) => distanciaParaMetros(a.distancia) - distanciaParaMetros(b.distancia))
      postos.value = normalizados

      if (postos.value.length === 0) {
        erro.value = 'Nenhum posto de vacinação encontrado nas proximidades.'
      }
    } else if (resp.status === 429 && !jaRepetiu) {
      await new Promise(r => setTimeout(r, 800))
      await buscarPostosPorUsuario(usuarioId, true)
      return
    } else if (resp.status === 400) {
      erro.value = typeof resp.data === 'string'
        ? resp.data
        : 'Não foi possível determinar o endereço do usuário.'
    } else if (resp.status === 404) {
      erro.value = 'Usuário não encontrado.'
    } else if (resp.status >= 500) {
      erro.value = 'Falha ao consultar o serviço. Tente novamente em instantes.'
    } else {
      erro.value = typeof resp.data === 'string'
        ? resp.data
        : `Erro ao buscar postos (HTTP ${resp.status}).`
    }
  } catch (e) {
    if (axios.isCancel?.(e)) {
      return;
    } else {
      console.error(e)
      erro.value = 'Erro de rede ao consultar o servidor.'
    }
  } finally {
    finalizarProgresso()
    carregando.value = false
  }
}

onMounted(async () => {
  nomeUsuario.value = localStorage.getItem('usuarioNome') || 'Usuário'
  const usuarioId = localStorage.getItem('usuarioId')
  if (!usuarioId) {
    erro.value = 'Não foi possível identificar o usuário. Faça login novamente.'
    return
  }
  await buscarPostosPorUsuario(Number(usuarioId))
})

onBeforeUnmount(() => {
  clearInterval(timer)
  if (controller) controller.abort()
})
</script>

<style scoped>
.pagina-postos {
  margin-left: 95px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 2rem;
  background: #fff;
  border-bottom: 1px solid #eee;
}

.titulo {
  font-size: 1.8rem;
  font-weight: bold;
  color: #f97316;
}

.usuario {
  display: flex;
  align-items: center;
}

.breadcrumb-link {
  color: #6b7280;
  transition: color .2s;
  font-size: 1.1rem;
}

.breadcrumb-link:hover {
  color: #f97316;
  text-decoration: underline;
}

.breadcrumb-laranja {
  color: #f97316 !important;
  font-weight: 900;
  font-size: 1.1rem;
}

.breadcrumb-home-img {
  margin-top: -5px;
}

.conteudo {
  background: white;
  min-height: calc(100vh - 160px);
}

.lista {
  max-width: 1150px;
  margin: 0 auto;
  margin-left: -2px;
}

.m3-card {
  border-radius: 12px;
  transition: box-shadow .15s ease, transform .05s ease, background .2s ease;
  min-height: 110px;
}

.m3-card:hover {
  box-shadow: none;
}

.m3-card:active {
  transform: none;
}

.card-conteudo {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  padding: 12px 18px;
  gap: 12px;
  margin-left: -10px;
}

.texto {
  min-width: 0;
}

.m3-card__title {
  font-weight: 700;
  color: #1f2937;
  font-size: 1rem;
  line-height: 1.3;
}

.m3-card__subtitle {
  color: #6b7280;
  font-size: .9rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.distancia {
  font-size: .85rem;
  color: #f97316;
  margin-top: 2px;
  margin-left: 12px;
}

.seta {
  width: 20px;
  height: 20px;
  opacity: .7;
}

.loading-card {
  background: rgb(255, 119, 0);
  color: white;
  padding: 16px 20px;
  border-radius: 12px;
  min-width: 220px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

/* Mobile */
@media (max-width: 600px) {
  .pagina-postos {
    margin-left: 95px;
  }
}
</style>