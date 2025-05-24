import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import CriarConta from "@/views/CriarConta.vue";
import Home from "../views/HomeView.vue";
import EsqueciMinhaSenha from "@/views/EsqueciMinhaSenha.vue";
import RedefinirSenha from "@/views/RedefinirSenha.vue";
import VacinasMundo from '@/views/VacinaObrigatoriasPaises.vue';

const routes = [
  { path: "/", component: Login },
  { path: "/criar-conta", component: CriarConta },
  { path: "/home", component: Home },
  {path: "/esqueci-minha-senha", component: EsqueciMinhaSenha},
  { path: '/redefinir-senha', component: RedefinirSenha },
  { path: '/vacinas-mundo', component: VacinasMundo },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
