import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import CriarConta from "@/views/CriarConta.vue";
import Home from "../views/HomeView.vue";
import EsqueciMinhaSenha from "@/views/EsqueciMinhaSenha.vue";
import RedefinirSenha from "@/views/RedefinirSenha.vue";
import VacinasMundo from '@/views/VacinaObrigatoriasPaises.vue';
import ComprovanteView from "@/views/ComprovanteView.vue";
import Configuracoes from '@/views/AcessarConfiguracoes.vue';
import HistoricoNotificacoes from "@/views/HistoricoNotificacoes.vue";
import EdicaoUsuario from "@/views/EdicaoUsuario.vue";

const routes = [
  { path: "/", component: Login },
  { path: "/criar-conta", component: CriarConta },
  { path: "/home", component: Home },
  {path: "/esqueci-minha-senha", component: EsqueciMinhaSenha},
  { path: '/redefinir-senha', component: RedefinirSenha },
  { path: '/vacinas-mundo', component: VacinasMundo },
  {path: '/exportar-comprovante', component: ComprovanteView},
  { path: '/vacinas', component: Configuracoes },
  {path: '/historico-notificacoes', component: HistoricoNotificacoes},
  {path: '/editar-perfil', component: EdicaoUsuario},
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
