import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import CriarConta from "@/views/CriarConta.vue";

const routes = [
  { path: "/", component: Login },
  { path: "/criar-conta", component: CriarConta },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
