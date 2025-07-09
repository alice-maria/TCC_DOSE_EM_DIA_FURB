<template>
    <v-container fluid class="pa-0">
        <div class="pagina-poliprivacidade">
            <!-- Cabeçalho -->
            <div class="header">
                <h1 class="titulo" @click="$router.push('/home')">Dose em dia</h1>
                <div class="usuario">
                    <img src="@/imagens/UserPhoto.png" alt="Ícone de usuário" class="icone-usuario"
                        @click="$router.push('/editar-perfil')" />
                    <span class="saudacao" @click="$router.push('/editar-perfil')">Olá, {{ nomeUsuario }}!</span>
                </div>
            </div>

            <!-- Breadcrumbs -->
            <v-breadcrumbs :items="breadcrumbs" class="meus-breadcrumbs px-6">
                <template v-slot:item="{ item, index }">
                    <span :class="[
                        'breadcrumb-link',
                        index === breadcrumbs.length - 1 ? 'breadcrumb-laranja' : ''
                    ]" @click="item.to && navegar(item.to)" style="cursor: pointer;">
                        <v-icon left small v-if="item.icon">{{ item.icon }}</v-icon>
                        {{ item.text }}
                    </span>
                </template>
            </v-breadcrumbs>
        </div>
    </v-container>
</template>

<script>

export default {
    data() {
        return {
            nomeUsuario: "",
            breadcrumbs: [
                { text: 'Serviços e Informações', to: '/home', icon: 'mdi-home' },
                { text: 'Configurações', to: '/configuracoes' },
                { text: 'Politica de Privacidade' }
            ]
        };
    },
    methods: {
        navegar(rota) {
            if (rota) {
                this.$router.push(rota);
            }
        },
    },
    mounted() {
        this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
    }
};
</script>

<style scoped>
.pagina-poliprivacidade {
    margin-left: 95px;
}

.header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem 2rem;
    background-color: white;
    border-bottom: 1px solid #eee;
}

.titulo {
    font-size: 1.8rem;
    font-weight: bold;
    color: #f97316;
}

.usuario {
    display: flex;
    align-items: center;
}

.icone-usuario {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    margin-right: 0.5rem;
}

.saudacao {
    font-weight: 500;
}

.breadcrumb-link {
    color: #6b7280;
    /* cinza padrão */
    transition: color 0.2s;
    font-size: 1.1rem;
}

.breadcrumb-link:hover {
    color: #f97316;
    /* hover laranja */
    text-decoration: underline;
}

.breadcrumb-laranja {
    color: #f97316 !important;
    /* laranja fixo no último */
    font-weight: 900;
    font-size: 1.1rem;
}
</style>