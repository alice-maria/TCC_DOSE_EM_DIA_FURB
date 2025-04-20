import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import CriarConta from "@/views/CriarConta.vue";
import Home from "../views/HomeView.vue";

const routes = [
  { path: "/", component: Login },
  { path: "/criar-conta", component: CriarConta },
  { path: "/home", component: Home },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
