<template>
    <div class="container mx-auto p-4 bg-white rounded-lg shadow">
        <!-- Cabeçalho -->
        <div class="flex justify-between items-center mb-4 px-4">
            <h1 class="text-2xl font-bold text-orange-600">Dose em dia</h1>
            <div class="flex items-center gap-4">
                <v-avatar color="white" size="40">
                    <v-icon class="text-orange-600">mdi-account-circle</v-icon>
                </v-avatar>
                <span>Olá, {{ nomeUsuario }}</span>
            </div>
        </div>

        <!-- Navegação -->
        <div class="flex justify-between items-center text-sm mb-4 px-4">
            <div class="flex items-center">
                <v-icon class="mr-2">mdi-home</v-icon>
                <span class="text-black">Serviços e Informações</span>
                <span class="mx-1">&gt;</span>
                <span class="font-semibold text-orange-600">Vacinas pelo mundo</span>
            </div>
            <div class="flex items-center" style="float: right;">
                <v-btn variant="outlined" color="primary" class="text-orange-600 border-orange-600" @click="mostrarFiltro = !mostrarFiltro">
                    <span class="font-semibold">FILTROS</span>
                </v-btn>
            </div>
        </div>

        <!-- Filtro em linha completa -->
        <div v-if="mostrarFiltro" class="w-full mb-4 px-4">
            <v-text-field v-model="filtro" label="Pesquisar país..." outlined clearable class="w-full"></v-text-field>
        </div>

        <!-- Lista de Países -->
        <v-card class="rounded-xl p-4">
            <v-list>
                <template v-for="(pais, index) in paisesFiltrados" :key="pais.idPais">
                    <v-list-item @click="redirecionarUrl(pais.url)" class="cursor-pointer hover:bg-gray-100 rounded-lg">
                        <v-list-item-content>
                            <v-list-item-title class="text-orange-600 font-bold text-lg">{{ pais.nome }}</v-list-item-title>
                            <v-list-item-subtitle class="text-gray-500">Consulte as vacinas desse país</v-list-item-subtitle>
                        </v-list-item-content>
                        <v-list-item-icon>
                            <v-icon class="text-orange-600 text-2xl">mdi-chevron-right</v-icon>
                        </v-list-item-icon>
                    </v-list-item>
                    <v-divider v-if="index < paisesFiltrados.length - 1"></v-divider>
                </template>
            </v-list>
        </v-card>

        <p v-if="paisesFiltrados.length === 0" class="text-gray-500">Nenhum país encontrado.</p>
    </div>
</template>

<script>
import axios from "axios";

export default {
    data() {
        return {
            paises: [],
            nomeUsuario: localStorage.getItem('nomeUsuario') || 'Usuário',
            filtro: "",
            mostrarFiltro: false
        };
    },
    computed: {
        paisesFiltrados() {
            return this.filtro.trim() === "" ? this.paises : this.paises.filter(pais => pais.nome.toLowerCase().includes(this.filtro.toLowerCase()));
        }
    },
    mounted() {
        this.carregarPaises();
    },
    methods: {
        async carregarPaises() {
            try {
                const response = await axios.get("http://localhost:5054/api/paises/listaPaises");
                this.paises = response.data.$values || [];
            } catch (error) {
                this.paises = [];
            }
        },
        redirecionarUrl(url) {
            if (url) window.open(url, "_blank");
        }
    }
};
</script>

<style scoped>
.container {
    background-color: #f8f9fa;
    width: 100%;
    max-width: 1400px;
    min-height: 80vh;
    padding: 2rem;
}

.v-list-item {
    transition: background-color 0.2s;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 16px;
    border-radius: 12px;
}

.v-list-item-title {
    color: #f97316;
    font-weight: bold;
}

.v-list-item-subtitle {
    color: #6b7280;
}

.v-icon {
    font-size: 1.5rem;
    color: #f97316 !important;
}

.text-orange-600 {
    color: #f97316 !important;
}
</style>