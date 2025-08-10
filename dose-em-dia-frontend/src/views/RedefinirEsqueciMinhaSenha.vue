<template>
  <v-container fluid class="pa-0">
    <div class="pagina-redefinirSenha">
      <div class="form-senha">
        <h3 class="text-orange font-weight-bold mb-1">Segurança</h3>
        <p class="text-grey mb-6">Altere aqui a sua senha.</p>
      </div>

      <v-form @submit.prevent="redefinirSenha" class="form-senha">
        <!-- Senha atual -->
        <v-text-field v-model="form.senhaAtual" label="Senha atual:*" variant="outlined"
          :type="mostrarSenhaAtual ? 'text' : 'password'" required>
          <template #append-inner>
            <img :src="mostrarSenhaAtual ? iconeOlhoAberto : iconeOlhoFechado" class="icone-olho-custom"
              @click.stop="mostrarSenhaAtual = !mostrarSenhaAtual" />
          </template>
        </v-text-field>

        <!-- Nova senha -->
        <v-text-field v-model="form.senha" label="Nova senha:*" variant="outlined"
          :type="mostrarNovaSenha ? 'text' : 'password'" required @input="validarSenha"
          :error="form.senha.length > 0 && !senhaValida">
          <template #append-inner>
            <img :src="mostrarNovaSenha ? iconeOlhoAberto : iconeOlhoFechado" class="icone-olho-custom"
              @click.stop="mostrarNovaSenha = !mostrarNovaSenha" />
          </template>
        </v-text-field>

        <p v-if="form.senha.length > 0 && !senhaValida" class="text-danger-senha">
          A senha deve conter no mínimo 8 caracteres e ao menos 1 letra maiúscula, 1 número e 1 caractere especial.
        </p>

        <!-- Confirmação -->
        <v-text-field v-model="form.confirmarSenha" label="Confirme sua senha:*" variant="outlined"
          :type="mostrarConfirmarSenha ? 'text' : 'password'" required
          :error="form.confirmarSenha.length > 0 && form.confirmarSenha !== form.senha">
          <template #append-inner>
            <img :src="mostrarConfirmarSenha ? iconeOlhoAberto : iconeOlhoFechado" class="icone-olho-custom"
              @click.stop="mostrarConfirmarSenha = !mostrarConfirmarSenha" />
          </template>
        </v-text-field>

        <p v-if="form.confirmarSenha.length > 0 && form.confirmarSenha !== form.senha" class="text-danger-senha">
          As senhas não coincidem.
        </p>
      </v-form>
    </div>

    <v-row justify="center" class="mt-4">
      <v-btn class="btn-salvar" style="min-width: 200px;" @click="dialogConfirmacao = true">
        Salvar
      </v-btn>
    </v-row>
  </v-container>

  <v-dialog v-model="dialogConfirmacao" max-width="400" persistent>
    <v-card class="popup-confirmacao">
      <v-card-text class="texto-confirmacao">
        Você confirma a alteração de senha?
      </v-card-text>
      <div class="botoes-popup">
        <v-btn class="btn-cancelar" @click="dialogConfirmacao = false">Cancelar</v-btn>
        <v-btn class="btn-confirmar" @click="confirmarAlteracaoSenha">Confirmar</v-btn>
      </div>
    </v-card>
  </v-dialog>
  <!-- POPUP DE SUCESSO -->
  <v-dialog v-model="dialogSucesso" max-width="400" persistent>
    <v-card class="popup-sucesso">
      <v-card-text class="texto-sucesso">
        Senha alterada com sucesso!
      </v-card-text>
      <v-card-actions class="botoes-popup">
        <v-btn class="btn-popupok" @click="$router.push('/')">Ok</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- POPUP DE ERRO -->
  <v-dialog v-model="mostrarErro" max-width="400">
    <v-card>
      <v-alert type="error" color="red-darken-2" icon="mdi-alert-circle" class="pa-5" border="start" elevation="2"
        title="Erro ao salvar">
        {{ mensagem }}
      </v-alert>
      <v-card-actions class="justify-end">
        <v-btn color="red-darken-2" variant="flat" @click="mostrarErro = false">OK</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

</template>

<script>
import axios from 'axios';

export default {
  name: 'RedefinirSenha',
  data() {
    return {
      mensagem: '',
      mostrarSenhaAtual: false,
      mostrarNovaSenha: false,
      mostrarConfirmarSenha: false,
      senhaValida: false,
      iconeOlhoAberto: require('@/assets/icons/eyes-on.svg'),
      iconeOlhoFechado: require('@/assets/icons/eyes-off.svg'),
      dialogConfirmacao: false,
      dialogSucesso: false,
      mostrarErro: false,
      form: {
        senhaAtual: '',
        senha: '',
        confirmarSenha: '',
        email: ''
      },
    };
  },
  methods: {
    async redefinirSenha() {
      if (this.novaSenha !== this.confirmarSenha) {
        this.mensagem = 'As senhas não coincidem.';
        this.mostrarErro = true;
        return;
      }

      if (!this.senhaValida) {
        this.mensagem = 'A nova senha não atende aos requisitos.';
        this.mostrarErro = true;
        return;
      }

      try {
        await axios.put('http://localhost:5054/api/usuario/alterar-senha', {
          email: this.form.email,
          senhaAtual: this.form.senhaAtual,
          novaSenha: this.form.senha
        }, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`
          }
        });

        this.dialogSucesso = true;
      } catch (err) {
        this.mensagem = err.response?.data || 'Erro ao alterar a senha.';
        this.mostrarErro = true;
      }
    },
    navegar(destino) {
      this.$router.push(destino);
    },
    validarSenha() {
      const senha = this.form.senha;

      let letraMaiscula = 0;
      let numero = 0;
      let caracterEspecial = 0;
      const caracteresEspeciais = "/([~`!@#$%^&*+=\\-\\[\\]\\\\';,/{}|\":<>?])";

      //usar valores ASCII melhora a performance ;)
      for (let i = 0; i < senha.length; i++) {
        const valorAscii = senha.charCodeAt(i);

        // Verifica se é letra maiúscula (A-Z)
        if (valorAscii >= 65 && valorAscii <= 90) {
          letraMaiscula++;
        }

        // Verifica se é número (0-9)
        if (valorAscii >= 48 && valorAscii <= 57) {
          numero++;
        }

        // Verifica se é caractere especial
        if (caracteresEspeciais.indexOf(senha[i]) !== -1) {
          caracterEspecial++;
        }
      }

      // Verifica se atende aos requisitos
      this.senhaValida = (senha.length >= 7) &&
        (letraMaiscula >= 1) &&
        (numero >= 1) &&
        (caracterEspecial >= 1);
    },
    confirmarAlteracaoSenha() {
      this.dialogConfirmacao = false;
      this.redefinirSenha(); // chama o método real de alteração
    }
  }
};
</script>

<style scoped>
.pagina-redefinirSenha {
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

.text-danger-senha {
  color: #dc2626;
  font-size: 0.875rem;
  margin-top: -10px;
  margin-bottom: 16px;
}

.icone-olho-custom {
  width: 24px;
  height: 24px;
  cursor: pointer;
}

.form-senha {
  margin-left: 25px;
  max-width: 1500px;
}

.botao {
  border-radius: 30px;
  font-weight: bold;
  font-size: 16px;
  height: 48px;
  text-transform: none;
}

.btn-salvar {
  background-color: #f97316;
  color: white;
  border-radius: 30px;
  font-weight: bold;
  text-transform: none;
}

.popup-confirmacao {
  background-color: #f97316;
  border-radius: 20px;
  padding: 20px;
  text-align: center;
}

.texto-confirmacao {
  color: white;
  font-weight: bold;
  font-size: 1.2rem;
  margin-bottom: 16px;
}

.botoes-popup {
  display: flex;
  justify-content: center;
  gap: 12px;
}

.btn-cancelar {
  background-color: #fda65b;
  color: white;
  font-weight: bold;
  border-radius: 999px;
  text-transform: none;
}

.btn-confirmar {
  background-color: white;
  color: #f97316;
  font-weight: bold;
  border-radius: 999px;
  text-transform: none;
}

.popup-sucesso {
  background-color: #f46c20;
  border-radius: 24px !important; /* forçar arredondamento */
  padding: 40px 20px;
  width: 300px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
  text-align: center;
}

.texto-sucesso {
  color: white;
  font-weight: bold;
  font-size: 1.2rem;
  text-align: center;
}

.botoes-popup {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}

.btn-popupok{
  display: flex;
  justify-content: center;
  margin-top: 20px;
}
</style>