<!-- Sidebar.vue -->
<template>
  <div>
    <!-- Overlay -->
    <div v-if="!isCollapsed" class="sidebar-overlay" @click="toggleSidebar"></div>

    <!-- Sidebar -->
    <aside :class="['position-fixed h-100 bg-primary', { 'w-15': !isCollapsed, 'w-auto': isCollapsed }]" style="background-color: #f46c20 !important;">
      <div class="menu-toggle">
        <button @click="toggleSidebar" class="btn-toggle-menu">
          <i class="fas fa-bars"></i>
        </button>
        <span v-if="!isCollapsed" class="text-white ms-2 fw-bold">Menu</span>
      </div>

      <!-- Menu Items -->
      <ul class="list-unstyled px-3">
        <li v-for="item in menuItems" :key="item.title" class="mb-2">
          <div
            class="d-flex align-items-center text-white text-decoration-none p-2 rounded hover-effect"
            @click="handleMenuItem(item)"
            style="cursor: pointer;"
          >
            <i :class="item.icon"></i>
            <span v-if="!isCollapsed" class="ms-3">{{ item.title }}</span>
          </div>
        </li>
      </ul>
    </aside>

    <!-- Diálogo de confirmação de saída -->
    <v-dialog v-model="showLogoutDialog" max-width="320">
      <v-card class="popup-sair">
        <v-card-text class="popup-sair__texto">
          Você confirma a saída da conta?
        </v-card-text>
        <v-card-actions class="popup-sair__botoes">
          <v-btn class="popup-sair__cancelar" @click="showLogoutDialog = false">Cancelar</v-btn>
          <v-btn class="popup-sair__confirmar" @click="confirmarLogout">Confirmar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
export default {
  data() {
    return {
      isCollapsed: true,
      showLogoutDialog: false,
      menuItems: [
        { title: 'Início', icon: 'fas fa-home', route: '/home' },
        { title: 'Configurações', icon: 'fas fa-cog', route: '/configuracoes' },
        { title: 'Exportar Comprovante', icon: 'fas fa-file-pdf', route: '/exportar-comprovante' },
        { title: 'Vacinas pelo Mundo', icon: 'fas fa-globe-americas', route: '/vacinas-mundo' },
        { title: 'Postos de Saúde', icon: 'fas fa-map-marker-alt', route: '/postos-saude' },
        { title: 'Notificações', icon: 'fas fa-bell', route: '/historico-notificacoes' },
        { title: 'Meu perfil', icon: 'fas fa-user', route: '/editar-perfil' },
        { title: 'Sair', icon: 'fas fa-sign-out-alt', action: 'logout' }
      ]
    };
  },
  methods: {
    toggleSidebar() {
      this.isCollapsed = !this.isCollapsed;
    },
    handleMenuItem(item) {
      if (item.action === 'logout') {
        this.showLogoutDialog = true;
      } else if (item.route) {
        this.$router.push(item.route);
      }
    },
    confirmarLogout() {
      localStorage.removeItem('token');
      localStorage.removeItem('usuarioNome');
      this.$router.push('/');
    }
  }
};
</script>

<style scoped>
.hover-effect:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 999;
}

aside {
  z-index: 1000;
  transition: width 0.3s ease;
}

@media (max-width: 768px) {
  aside {
    width: 100% !important;
  }
}

.w-15 {
  width: 15% !important;
}

.menu-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 70px;
}

.btn-toggle-menu {
  background: none;
  border: none;
  color: white;
  font-size: 25px !important;
  cursor: pointer;
}

/* Estilo do pop-up de saída */
.popup-sair {
  background-color: #f97316;
  border-radius: 25px !important;
  padding: 32px 24px;
  color: white;
  text-align: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.popup-sair__texto {
  font-weight: bold;
  font-size: 1.3rem !important;
  margin-bottom: 24px;
  line-height: 1.5;
}

.popup-sair__botoes {
  display: flex;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
}

.popup-sair__cancelar,
.popup-sair__confirmar {
  border-radius: 999px;
  text-transform: none;
  font-weight: 600;
  padding: 8px 20px;
  font-size: 0.95rem;
  min-width: 100px;
}

.popup-sair__cancelar {
  background-color: #fb923c;
  color: white;
}

.popup-sair__confirmar {
  background-color: white;
  color: #f97316;
}
</style>