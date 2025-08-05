<template>
    <v-container fluid class="pa-0">
        <!-- Envoltório com margem -->
        <div class="pagina-paises usa-escala">
            <!-- Cabeçalho -->
            <div class="header">
                <h1 class="titulo" @click="$router.push('/home')">Dose em dia</h1>
                <div class="usuario">
                    <img src="@/imagens/UserPhoto.png" alt="Ícone de usuário" class="icone-usuario"
                        @click="$router.push('/editar-perfil')" />
                    <span class="saudacao" @click="$router.push('/editar-perfil')">Olá, {{ nomeUsuario }}!</span>
                </div>
            </div>

            <!-- Breadcrumbs -->
            <v-breadcrumbs class="meus-breadcrumbs px-6" :items="breadcrumbs">
                <template v-slot:item="{ item }">
                    <span :class="['breadcrumb-link', { 'breadcrumb-laranja': !item.to }]"
                        @click="item.to && navegar(item.to)" style="cursor: pointer;">
                        <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt=""
                            class="breadcrumb-home-img" />
                        {{ item.text }}
                    </span>
                </template>
            </v-breadcrumbs>

            <!-- Navegação e Filtro -->
            <div class="navegacao-filtro">
                <v-btn class="botao-round-laranja" @click="mostrarFiltro = !mostrarFiltro">
                    Filtro
                </v-btn>

            </div>

            <!-- Filtro em linha completa -->
            <div v-if="mostrarFiltro" class="filtro">
                <v-text-field v-model="filtro" placeholder="Pesquisar país" prepend-inner-icon="mdi-magnify" clearable
                    rounded variant="outlined" density="comfortable" hide-details
                    class="campo-pesquisa campo-material-search" />
            </div>

            <!-- Lista de Países -->
            <div class="lista-paises rounded-xl px-3">
                <v-list>
                    <template v-for="(pais, index) in paisesFiltrados" :key="pais.idPais">
                        <v-list-item @click="redirecionarUrl(pais.url)" class="item-pais hover-sombra" lines="two">
                            <v-list-item-content>
                                <v-list-item-title class="titulo-pais">{{ pais.nome }}</v-list-item-title>
                                <v-list-item-subtitle class="subtitulo-pais">Consulte as vacinas desse
                                    país.</v-list-item-subtitle>
                            </v-list-item-content>
                            <template #append>
                                <img src="@/assets/icons/seta.svg" alt="excluir" class="icones" />
                            </template>
                        </v-list-item>
                        <v-divider v-if="index < paisesFiltrados.length - 1"></v-divider>
                    </template>
                </v-list>
            </div>

            <p v-if="paisesFiltrados.length === 0" class="mensagem-nenhum-pais">Nenhum país encontrado.</p>
        </div>
    </v-container>
</template>

<script>
import axios from "axios";

export default {
    data() {
        return {
            paises: [],
            nomeUsuario: "",
            filtro: "",
            mostrarFiltro: false,
            breadcrumbs: [
                { text: "Serviços e Informações", to: "/home", icon: "mdi-home" },
                { text: "Vacinas pelo mundo" }
            ],
        };
    },
    computed: {
        paisesFiltrados() {
            return this.filtro.trim() === "" ? this.paises : this.paises.filter(pais => pais.nome.toLowerCase().includes(this.filtro.toLowerCase()));
        }
    },
    mounted() {
        this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
        this.carregarPaises();
    },

    methods: {
        async carregarPaises() {
            try {
                const response = await axios.get("http://localhost:5054/api/paises/listaPaises");
                this.paises = Array.isArray(response.data) ? response.data : response.data.$values || [];
            } catch (error) {
                this.paises = [];
            }
        },
        redirecionarUrl(url) {
            if (url) window.open(url, "_blank");
        },
        navegar(rota) {
            if (rota) this.$router.push(rota);
        },
    }
};
</script>

<style scoped>
.pagina-paises {
    margin-left: 95px;
}

.header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem 2rem;
    background-color: white;
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

.icone-usuario {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    margin-right: 0.5rem;
}

.saudacao {
    font-weight: 500;
}

.navegacao-filtro {
    display: flex;
    justify-content: flex-end;
    /* Alinha botão à direita */
    padding: 0 1rem;
    margin-top: 0.5rem;
}


.navegacao {
    display: flex;
    align-items: center;
}

.botao-round-laranja {
    background-color: transparent;
    border: 1.5px solid #f97316;
    color: #f97316;
    border-radius: 999px;
    font-weight: 600;
    font-size: 0.875rem;
    text-transform: none;
    padding: 6px 20px;
    min-height: 40px;
    min-width: 80px;
    transition: background-color 0.2s ease;
}

.lista-paises {
    margin-top: 16px;
}

.item-pais {
    align-items: flex-start;
    padding-block: 14px;
    transition: background-color 0.3s, box-shadow 0.3s;
}

.hover-sombra {
    cursor: pointer;
    transition: background-color 0.2s ease;
    min-height: 12vh;
}

.hover-sombra:hover {
    background-color: #f9f9f9;
}

.titulo-pais {
    font-weight: 600;
    color: #f97316;
    line-height: 1.2;
    margin-bottom: 6px;
    font-size: clamp(1.35rem, 0.9vw + 1.0rem, 1.8rem);
}

.subtitulo-pais {
    font-size: 0.95rem;
    color: #555;
}

:deep(.v-list-item__content),
:deep(.v-list-item-subtitle) {
    font-size: 0.9em !important;
}

:deep(.v-list-item-title),
:deep(.v-list-item){
    font-size: 1.2em !important;
}

:deep(.v-list-item-title),
:deep(.v-list-item-subtitle) {
    white-space: normal !important;
    overflow: visible !important;
    text-overflow: clip !important;
    display: block !important;
    -webkit-line-clamp: unset !important;
    line-clamp: unset !important;
    -webkit-box-orient: unset !important;
}

:deep(.v-list-item__content) {
    overflow: visible !important;
}

.lista-paises :deep(.v-divider) {
    margin: 10px 0;
}

.mensagem-nenhum-pais {
    text-align: center;
    color: #6b7280;
}

.breadcrumb-link {
    color: #6b7280;
    transition: color 0.2s;
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
</style>