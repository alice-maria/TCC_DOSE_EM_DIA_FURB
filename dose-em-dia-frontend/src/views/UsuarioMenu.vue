<template>
  <v-menu v-model="menuAberto" location="bottom end" :offset="8" content-class="usuario-menu-content">
    <template #activator="{ props }">
      <div v-bind="props" class="usuario d-flex align-items-center gap-2" style="cursor: pointer;">
        <img src="@/imagens/UserPhoto.png" alt="Ícone de usuário" class="icone-usuario" />
        <span class="saudacao">Olá, {{ nomeUsuario }}!</span>
      </div>
    </template>

    <v-list>
      <v-list-item @click="navegarEditarPerfil">
        <template #prepend>
          <img src="@/assets/icons/perfil.svg" alt="Ícone de perfil" class="icone" />
        </template>
        <v-list-item-title>Editar informações</v-list-item-title>
      </v-list-item>

      <v-list-item @click="abrirDialogSair">
        <template #prepend>
          <img src="@/assets/icons/sair.svg" alt="Ícone de sair" class="icone" />
        </template>
        <v-list-item-title>Sair da conta</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-menu>

  <!-- Diálogo de confirmação (fora do v-menu) -->
  <v-dialog v-model="dialogSair" max-width="360" persistent>
    <v-card class="popup-sair">
      <v-card-text class="popup-sair__texto">
        Você confirma a saída da conta?
      </v-card-text>
      <v-card-actions class="popup-sair__botoes">
        <v-btn class="popup-sair__cancelar" variant="flat" @click="dialogSair = false">Cancelar</v-btn>
        <v-btn class="popup-sair__confirmar" variant="flat" @click="confirmarSaida">Confirmar</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
export default {
  name: 'UsuarioMenu',
  data() {
    return {
      nomeUsuario: localStorage.getItem('usuarioNome') || 'Usuário',
      menuAberto: false,
      dialogSair: false,
    };
  },
  methods: {
    navegarEditarPerfil() {
      this.menuAberto = false;
      this.$router.push('/editar-perfil');
    },
    abrirDialogSair() {
      this.menuAberto = false;        
      this.dialogSair = true;
    },
    confirmarSaida() {
      // limpeza do estado local
      localStorage.removeItem('token');
      localStorage.removeItem('usuarioNome');
      localStorage.removeItem('usuarioCpf');

      this.dialogSair = false;
      this.$router.push('/');
    },
  },
};
</script>

<style scoped>
.icone-usuario {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.saudacao {
  font-weight: 500;
  color: #000;
}

.icone {
  width: 20px;
  height: 20px;
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

@media (max-width: 600px) {
  .icone-usuario {
    width: 28px;
    height: 28px;
  }

  .usuario {
    min-width: 0;
    max-width: 80vw;
    gap: 6px;
    align-items: center;
  }

  .saudacao {
    display: none !important;
  }

  .v-dialog>.v-overlay__content {
    align-items: center !important;
    justify-content: center !important;
  }

  .v-dialog {
    align-items: center !important;
  }

  .popup-sair {
    width: min(92vw, 420px);
    max-width: 420px;
    max-height: calc(100vh - 48px);
    margin: 0 auto;
    padding: 20px 16px;
    border-radius: 16px !important;
    overflow: auto;
  }

  .popup-sair__texto {
    font-size: 1.05rem !important;
    margin-bottom: 16px;
  }

  .popup-sair__botoes {
    gap: 10px;
  }

  .popup-sair__cancelar,
  .popup-sair__confirmar {
    width: 100%;
    min-width: 0;
    padding: 10px 14px;
    font-size: 1rem;
  }
}
</style>