<template>
  <v-menu offset-y>
    <template #activator="{ props }">
      <div v-bind="props" class="usuario d-flex align-items-center gap-2" style="cursor: pointer;">
        <img src="@/imagens/UserPhoto.png" alt="Ícone de usuário" class="icone-usuario" />
        <span class="saudacao">Olá, {{ nomeUsuario }}!</span>
      </div>
    </template>

    <v-list>
      <v-list-item @click="$router.push('/editar-perfil')">
        <v-list-item-title>Editar informações</v-list-item-title>
      </v-list-item>
      <v-list-item @click="logout">
        <v-list-item-title>Sair da conta</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<script>
export default {
  name: 'UsuarioMenu',
  data() {
    return {
      nomeUsuario: localStorage.getItem("usuarioNome") || "Usuário"
    };
  },
  methods: {
    logout() {
      localStorage.removeItem("token");
      localStorage.removeItem("usuarioNome");
      localStorage.removeItem("usuarioCPF");
      this.$router.push("/");
    }
  }
};
</script>

<style scoped>
.icone-usuario {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.saudacao {
  font-weight: 500;
  color: #000;
}
</style>
