<!-- Sidebar.vue -->
<template>
  <div>
    <!-- Overlay -->
    <div v-if="!isCollapsed" class="sidebar-overlay" @click="toggleSidebar"></div>

    <!-- Sidebar -->
    <aside :class="['sidebar', { 'sidebar--expanded': !isCollapsed, 'sidebar--collapsed': isCollapsed }]"
      style="background-color: #f46c20 !important;">
      <!-- Navigation Rail com Menu incluído -->
      <ul class="navigation-rail">
        <!-- Botão de Menu -->
        <li class="rail-item rail-toggle" @click="toggleSidebar">
          <div class="rail-content">
            <img :src="isCollapsed ? menuCloseIcon : menuOpenIcon" alt="Menu" class="rail-img-icon" />
          </div>
        </li>

        <!-- Demais Itens -->
        <li v-for="item in menuItems" :key="item.title"
          :class="['rail-item', { 'rail-item--active': isActiveRoute(item.route) }]" @click="handleMenuItem(item)">
          <!-- Wrapper para ícone + label -->
          <div class="rail-content">
            <!-- Ícone -->
            <template v-if="item.imgSrc">
              <img :src="item.imgSrc" alt="" class="rail-img-icon" />
            </template>
            <template v-else>
              <i :class="['rail-icon', item.icon]"></i>
            </template>

            <!-- Label -->
            <span v-if="!isCollapsed" class="rail-label">{{ item.title }}</span>
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
import menuOpenIcon from '@/assets/icons/sidebar/menu-aberto.svg';
import menuCloseIcon from '@/assets/icons/sidebar/menu-fechado.svg';

export default {
  data() {
    return {
      isCollapsed: true,
      showLogoutDialog: false,
      menuOpenIcon,   // colapsado
      menuCloseIcon,  // expandido
      menuItems: [
        { title: 'Início', imgSrc: require('@/assets/icons/sidebar/home.svg'), route: '/home' },
        { title: 'Configurações', imgSrc: require('@/assets/icons/sidebar/config.svg'), route: '/configuracoes' },
        { title: 'Exportar Comprovante', imgSrc: require('@/assets/icons/sidebar/pdf.svg'), route: '/exportar-comprovante' },
        { title: 'Vacinas pelo Mundo', imgSrc: require('@/assets/icons/sidebar/mundo.svg'), route: '/vacinas-mundo' },
        { title: 'Postos de Saúde', imgSrc: require('@/assets/icons/sidebar/localizacao.svg'), route: '/postos-saude' },
        { title: 'Notificações', imgSrc: require('@/assets/icons/sidebar/notificacao.svg'), route: '/historico-notificacoes' },
        { title: 'Meu perfil', imgSrc: require('@/assets/icons/sidebar/perfil.svg'), route: '/editar-perfil' },
        { title: 'Sair', imgSrc: require('@/assets/icons/sidebar/sair.svg'), action: 'logout' }
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
      this.isCollapsed = true;
    },
    confirmarLogout() {
      localStorage.removeItem('token');
      localStorage.removeItem('usuarioNome');
      this.$router.push('/');
    },
    isActiveRoute(route) {
      return this.$route.path === route;
    }
  }
};
</script>

<style scoped>
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

.sidebar {
  background-color: #f46c20 !important;
  position: fixed;
  top: 0;
  left: 0;
  height: 100%;
  z-index: 1000;
  transition: width 0.3s ease;
  display: flex;
  flex-direction: column;
}

.sidebar--expanded {
  width: 280px;
}

.sidebar--collapsed {
  width: 72px;
  /* padrão de rail colapsado (M3) */
}

@media (max-width: 768px) {
  .sidebar--expanded,
  .sidebar--collapsed {
    width: 100% !important;
  }
}

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

.navigation-rail {
  display: flex;
  flex-direction: column;
  height: 100%;
  justify-content: flex-start;
  padding-left: 0;
  padding-right: 0;
}

.rail-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 8px 0;
  margin-bottom: 6px;
  border-radius: 12px;
  color: white;
  cursor: pointer;
  width: 100%;
  transition: background-color 0.2s;
}

.rail-item:hover {
  background-color: rgba(255, 255, 255, 0.15);
}

.rail-item--active {
  background-color: rgba(255, 255, 255, 0.3);
}

.rail-icon {
  font-size: 20px;
}

.rail-label {
  max-width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  text-align: center;
}

.rail-toggle {
  margin-bottom: 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.rail-img-icon {
  width: 25px;
  height: 25px;
  object-fit: contain;
  margin-bottom: 4px;
}

.rail-content {
  display: flex;
  align-items: center;
  gap: 12px;
  justify-content: center;
  width: 100%;
  padding-left: 12px;
  transition: all 0.3s ease;
}

.sidebar--expanded .rail-content {
  justify-content: flex-start;
}
</style>