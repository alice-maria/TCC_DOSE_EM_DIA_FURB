import { createRouter, createWebHistory } from "vue-router";
import UserLogin from "../views/UserLogin.vue";
import CriarConta from "@/views/CriarConta.vue";
import Home from "../views/HomeView.vue";
import EsqueciMinhaSenha from "@/views/EsqueciMinhaSenha.vue";
import RedefinirSenha from "@/views/RedefinirSenha.vue";
import VacinasMundo from '@/views/VacinaObrigatoriasPaises.vue';
import ComprovanteView from "@/views/ComprovanteView.vue";
import Configuracoes from '@/views/AcessarConfiguracoes.vue';
import HistoricoNotificacoes from "@/views/HistoricoNotificacoes.vue";
import EdicaoUsuario from "@/views/EdicaoUsuario.vue";
import PostosSaude from "@/views/PostosSaude.vue";
import PoliticaPrivacidade from "@/views/PoliticaPrivacidade.vue";

const routes = [
  { path: "/", component: UserLogin },
  { path: "/criar-conta", component: CriarConta },
  { path: "/home", component: Home },
  {path: "/esqueci-minha-senha", component: EsqueciMinhaSenha},
  { path: '/redefinir-senha', component: RedefinirSenha },
  { path: '/vacinas-mundo', component: VacinasMundo },
  {path: '/exportar-comprovante', component: ComprovanteView},
  { path: '/configuracoes', component: Configuracoes },
  {path: '/historico-notificacoes', component: HistoricoNotificacoes},
  { path: '/redefinir-senha', name: 'redefinir-senha', component: RedefinirSenha },
  { path: '/editar-perfil', component: EdicaoUsuario },
  { path: '/informacoes-cadastrais', component: EdicaoUsuario },
  { path: '/postos-saude', component: PostosSaude },
    { path: '/politica-privacidade', component: PoliticaPrivacidade },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
