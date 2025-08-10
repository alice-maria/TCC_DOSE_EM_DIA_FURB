<template>
  <v-app>
    <div id="app" class="d-flex">
      <AppSidebar v-if="exibirSidebar" />
      <div class="flex-grow-1">
        <router-view />
      </div>
      <BotaoAcessibilidade />
      <ChatBot v-if="exibirChatBot" />
    </div>
  </v-app>
</template>

<script>
import AppSidebar from './components/AppSidebar.vue';
import BotaoAcessibilidade from './components/acessibilidade/BotaoAcessibilidade.vue';
import { useRoute } from 'vue-router';
import { ref, computed } from 'vue';
import ChatBot from "@/components/ChatBot.vue";

export default {
  components: {
    AppSidebar,
    BotaoAcessibilidade,
    ChatBot,
  },
  setup() {
    const route = useRoute();
    const rotasSemSidebar = ['/', '/criar-conta', '/esqueci-minha-senha', '/aceitar-politica-privacidade', '/esqueci-redefinir-minha-senha'];
    const exibirSidebar = computed(() =>
      !rotasSemSidebar.includes(route.path)
    );

    const mostrarChat = ref(false);
    const rotasSemChatBot = ['/', '/criar-conta', '/esqueci-minha-senha', '/aceitar-politica-privacidade', '/esqueci-redefinir-minha-senha'];
    const exibirChatBot = computed(() =>
      !rotasSemChatBot.includes(route.path)
    );

    return { exibirSidebar, exibirChatBot, mostrarChat };
  }
};
</script>

<style>
#app {
  min-height: 100vh;
}
</style>