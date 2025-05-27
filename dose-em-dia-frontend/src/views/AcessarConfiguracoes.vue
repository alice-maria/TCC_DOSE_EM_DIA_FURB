<template>
  <v-container class="pa-4">
    <!-- Cabeçalho -->
    <div class="header">
      <h1 class="titulo">Dose em dia</h1>
      <div class="usuario">
        <img src="@/imagens/icone-user-orange.png" alt="Ícone de usuário" class="icone-usuario">
        <span class="saudacao">Olá, {{ nomeUsuario }}</span>
      </div>
    </div>

    <v-breadcrumbs :items="breadcrumbs" class="mb-4" />

    <v-card class="card-ajustado">
      <v-card-text>
        <!-- Notificações -->
        <v-list-item class="hoverable">
          <v-list-item-content>
            <v-list-item-title class="secao-titulo">Notificações</v-list-item-title>
            <v-list-item-subtitle class="secao-descricao">
              Você receberá comunicações por e-mail sobre suas vacinas.
            </v-list-item-subtitle>
          </v-list-item-content>

          <template #append>
            <v-switch v-model="notificacoesAtivas" color="orange" />
          </template>
        </v-list-item>

        <v-divider class="separador"></v-divider>


        <!-- Informações Cadastrais -->
        <v-list-item @click="navegar('informacoes-cadastrais')" class="hoverable">
          <v-list-item-title class="secao-titulo">Informações cadastrais</v-list-item-title>
          <v-list-item-subtitle>Informações sobre seu perfil e dados cadastrais.</v-list-item-subtitle>
          <v-icon>mdi-chevron-right</v-icon>
        </v-list-item>
        <v-divider class="separador"></v-divider>

        <!-- Segurança -->
        <v-list-item @click="navegar('seguranca')" class="hoverable">
          <v-list-item-title class="secao-titulo">Segurança</v-list-item-title>
          <v-list-item-subtitle>Altere aqui a sua senha.</v-list-item-subtitle>
          <v-icon>mdi-chevron-right</v-icon>
        </v-list-item>
        <v-divider class="separador"></v-divider>

        <!-- Política de Privacidade -->
        <v-list-item @click="navegar('politica-privacidade')" class="hoverable">
          <v-list-item-title class="secao-titulo">Política de Privacidade</v-list-item-title>
          <v-list-item-subtitle>Regras de privacidade de dados pessoais.</v-list-item-subtitle>
          <v-icon>mdi-chevron-right</v-icon>
        </v-list-item>
        <v-divider class="separador"></v-divider>

        <!-- Sair -->
        <v-list-item @click="sair()" class="hoverable">
          <v-list-item-title class="secao-titulo">Sair</v-list-item-title>
          <v-list-item-subtitle>Desconecte da conta em que você está.</v-list-item-subtitle>
          <v-icon>mdi-logout</v-icon>
        </v-list-item>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const nomeUsuario = ref(localStorage.getItem('nomeUsuario') || 'Usuário')
const notificacoesAtivas = ref(true)

function sair() {
  localStorage.removeItem('token')
  localStorage.removeItem('nomeUsuario')
  router.push('/')
}

</script>

<style scoped>
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 3.0rem;
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

.separador {
  margin: 0.75rem 0;
}

.secao-titulo {
  font-size: 1.2rem;
  font-weight: 600;
  color: #f97316;
}

.secao-descricao {
  font-size: 0.9rem;
  color: #555;
}

.hoverable {
  cursor: pointer;
  transition: background-color 0.2s ease;
  min-height: 12vh; /* altura mínima */
}

.hoverable:hover {
  background-color: #f9f9f9;
}

.card-ajustado {
  width: 75vw; /* ocupa 95% da largura da viewport */
  min-height: 77vh; /* altura mínima */
  margin: 20px auto; /* centraliza com espaçamento */
  box-sizing: border-box;
}


</style>