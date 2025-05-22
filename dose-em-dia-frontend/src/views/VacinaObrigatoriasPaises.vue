<template>
    <div class="container">
        <!-- Cabeçalho -->
        <div class="header">
            <h1 class="titulo">Dose em dia</h1>
            <div class="usuario">
                <img src="@/imagens/icone-user-orange.png" alt="Ícone de usuário" class="icone-usuario">
                <span class="saudacao">Olá, {{ nomeUsuario }}</span>
            </div>
        </div>

        <!-- Navegação e Filtro -->
        <div class="navegacao-filtro">
            <div class="navegacao">
                <span class="texto">Serviços e Informações</span>
                <span class="seta">&gt;</span>
                <span class="pagina-atual">Vacinas pelo mundo</span>
            </div>
            <v-btn class="botao-filtro" variant="outlined" color="primary" @click="mostrarFiltro = !mostrarFiltro">
                <span>FILTROS</span>
            </v-btn>
        </div>

        <!-- Filtro em linha completa -->
        <div v-if="mostrarFiltro" class="filtro">
            <v-text-field v-model="filtro" label="Pesquisar país..." outlined clearable class="campo-pesquisa"></v-text-field>
        </div>

        <!-- Lista de Países -->
        <v-card class="lista-paises rounded-xl">
            <v-list>
                <template v-for="(pais, index) in paisesFiltrados" :key="pais.idPais">
                    <v-list-item @click="redirecionarUrl(pais.url)" class="item-pais hover-sombra">
                        <v-list-item-content>
                            <v-list-item-title class="titulo-pais">{{ pais.nome }}</v-list-item-title>
                            <v-list-item-subtitle class="subtitulo-pais">Consulte as vacinas desse país.</v-list-item-subtitle>
                        </v-list-item-content>
                        <v-list-item-icon>
                            <v-icon class="icone-seta">mdi-chevron-right</v-icon>
                        </v-list-item-icon>
                    </v-list-item>
                    <v-divider v-if="index < paisesFiltrados.length - 1"></v-divider>
                </template>
            </v-list>
        </v-card>

        <p v-if="paisesFiltrados.length === 0" class="mensagem-nenhum-pais">Nenhum país encontrado.</p>
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
    max-width: 1400px;
    min-height: 80vh;
    padding: 2rem;
}

.header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.titulo {
    font-size: 2rem;
    color: #f97316; /* Laranja */
    font-weight: bold;
}

.usuario {
    display: flex;
    align-items: center;
}

.icone-usuario {
    width: 27px;
    height: 27px;
}

.saudacao {
    margin-left: 8px;
}

.navegacao-filtro {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 8px;
}

.navegacao {
    display: flex;
    align-items: center;
}

.botao-filtro {
    margin-left: auto;
    color: #f97316 !important;
    border-color: #f97316 !important;
}

.texto {
    font-size: 1rem;
    color: #000000;
}

.seta {
    font-size: 1rem;
    color: #000000;
    margin: 0 4px;
}

.pagina-atual {
    font-size: 1rem;
    color: #f97316;
    font-weight: bold;
}

.filtro {
    margin-top: 8px;
}

.lista-paises {
    margin-top: 16px;
}

.item-pais {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 16px;
    border-radius: 8px;
    transition: background-color 0.3s, box-shadow 0.3s;
}

.hover-sombra:hover {
    background-color: #f1f1f1;
    box-shadow: 0 4px 30px rgba(0, 0, 0, 0.1);
}

.titulo-pais {
    font-size: 1.125rem;
    font-weight: bold;
    color: #f97316;
}

.subtitulo-pais {
    color: #6b7280;
}

.mensagem-nenhum-pais {
    text-align: center;
    color: #6b7280;
}
</style>