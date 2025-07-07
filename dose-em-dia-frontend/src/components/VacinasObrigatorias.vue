<template>
  <div class="container mx-auto p-4 bg-gray-100">
    <div class="header flex justify-between items-center mb-4">
      <h1 class="text-2xl font-bold text-orange-600">Dose em dia</h1>
      <div class="flex items-center gap-4">
        <span>Olá, {{ nomeUsuario }}</span>
        <button class="flex items-center gap-1 px-2 py-1 border rounded">Filtros</button>
      </div>
    </div>

    <div class="bg-white rounded-lg shadow p-4">
      <h2 class="text-lg font-semibold mb-2">Vacinas pelo mundo</h2>
      <ul>
        <li v-for="pais in paises" :key="pais.idPais" class="flex justify-between items-center p-2 border-b">
          <div>
            <span class="text-orange-600 font-bold">{{ pais.nome }}</span>
            <p class="text-gray-500">Consulte as vacinas desse país</p>
          </div>
          <button @click="redirecionarUrl(pais.url)" class="text-orange-600">&gt;</button>
        </li>
      </ul>
    </div>

    <div class="fixed right-4 bottom-4 flex flex-col gap-2">
      <button class="bg-orange-500 p-2 rounded-full text-white">+</button>
      <button class="bg-orange-500 p-2 rounded-full text-white">🔊</button>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      paises: []
    };
  },
  mounted() {
    this.carregarPaises();
  },
  methods: {
    async carregarPaises() {
      try {
        const response = await axios.get("http://localhost:5054/api/paises/listaPaises");
        this.paises = response.data;
      } catch (error) {
        console.error("Erro ao carregar os países:", error);
      }
    },
    redirecionarUrl(url) {
      if (url) {
        window.open(url, "_blank");
      }
    }
  }
};
</script>

<style scoped>
.container {
  max-width: 800px;
  margin: auto;
}
</style>