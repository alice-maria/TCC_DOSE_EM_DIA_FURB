<!-- eslint-disable vue/multi-word-component-names -->
<template>
  <div>
    <div v-if="!isCollapsed" class="sidebar-overlay" @click="toggleSidebar"></div>
    <aside :class="['position-fixed h-100 bg-primary', { 'w-15': !isCollapsed, 'w-auto': isCollapsed }]" style="background-color: #f46c20 !important;">
      <div class="menu-toggle">
        <button @click="toggleSidebar" class="btn-toggle-menu">
          <i class="fas fa-bars"></i>
        </button>
        <span v-if="!isCollapsed" class="text-white ms-2 fw-bold">Menu</span>
      </div>
      <ul class="list-unstyled px-3">
        <li v-for="item in menuItems" :key="item.title" class="mb-2">
          <router-link :to="item.route" class="d-flex align-items-center text-white text-decoration-none p-2 rounded hover-effect">
            <i :class="item.icon"></i>
            <span v-if="!isCollapsed" class="ms-3">{{ item.title }}</span>
          </router-link>
        </li>
      </ul>
    </aside>
  </div>
</template>

<script>
export default {
  data() {
    return {
      isCollapsed: true,
      menuItems: [
        { title: 'Início', icon: 'fas fa-home', route: '/home' },
       { title: 'Configurações', icon: 'fas fa-cog', route: '/configuracoes' },
        { title: 'Exportar Comprovante', icon: 'fas fa-file-pdf', route: '/exportar-comprovante' },
        { title: 'Vacinas pelo Mundo', icon: 'fas fa-globe-americas', route: '/vacinas-mundo' },
        { title: 'Postos de Saúde', icon: 'fas fa-map-marker-alt', route: '/postos-saude' },
        { title: 'Notificações', icon: 'fas fa-bell', route: '/historico-notificacoes' },
        { title: 'Meu perfil', icon: 'fas fa-user', route: '/editar-perfil' },
        { title: 'Sair', icon: 'fas fa-sign-out-alt', route: '/' }
      ]
    };
  },
  methods: {
    toggleSidebar() {
      this.isCollapsed = !this.isCollapsed;
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
</style>  