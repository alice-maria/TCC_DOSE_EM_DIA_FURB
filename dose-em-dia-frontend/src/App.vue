<template>
  <div id="app">
    <!-- Navbar ou Header se quiser -->
    <router-view />
    <!-- Footer se quiser -->
    <!-- Outros componentes e conteúdo -->
    <BotaoAcessibilidade />
  </div>
</template>

<script>
import BotaoAcessibilidade from './components/acessibilidade/BotaoAcessibilidade.vue';

export default {
  components: {
    BotaoAcessibilidade,
  },
};
</script>:contentReference[oaicite:22]{index=22}
