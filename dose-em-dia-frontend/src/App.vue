<template>
  <div id="app" class="d-flex">
    <Sidebar v-if="exibirSidebar" />

    <div class="flex-grow-1">
      <router-view />
    </div>

    <BotaoAcessibilidade />
  </div>
</template>

<script>
import Sidebar from './components/Sidebar.vue';
import BotaoAcessibilidade from './components/acessibilidade/BotaoAcessibilidade.vue';
import { useRoute } from 'vue-router';
import { computed } from 'vue';

export default {
  components: {
    Sidebar,
    BotaoAcessibilidade,
  },
  setup() {
    const route = useRoute();
    const rotasComSidebar = ['/home', '/vacinas', '/perfil']; //DEFINE AS ROTAS QUE DEVEM EXIBIR A SIDEBAR

    
    const exibirSidebar = computed(() =>
      rotasComSidebar.includes(route.path)
    );

    return {exibirSidebar};
  }    
};

</script>

<style>
/* Para garantir que a sidebar fique do lado e o conteúdo ao lado dela */
#app {
  min-height: 100vh;
}
</style>

