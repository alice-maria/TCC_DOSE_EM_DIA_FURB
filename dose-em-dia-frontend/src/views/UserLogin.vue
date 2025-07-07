<template>
  <v-container fluid class="login-container">
    <v-row no-gutters class="login-row">
      <!-- Coluna Esquerda -->
      <v-col cols="12" md="6" class="coluna-esquerda">
        <h1 class="titulo-principal">Dose em dia</h1>
        <h3 class="subtitulo">Bem-vindo!</h3>
        <p class="mensagem-criacao">
          Proteja sua saúde! Crie sua conta e mantenha sua vacinação em dia.
        </p>
        <router-link to="/criar-conta">
          <v-btn class="btn-criar-conta" rounded>
            Criar conta
          </v-btn>
        </router-link>
      </v-col>

      <!-- Coluna Direita -->
      <v-col cols="12" md="6" class="coluna-direita">
        <h3 class="subtitulo">Bem-vindo de volta!</h3>
        <p class="mensagem-login">
          Para conectar conosco, por favor faça login com suas informações.
        </p>

        <v-form class="formulario-login">
          <v-text-field v-model="email" label="E-mail*" prepend-inner-icon="mdi-email" type="email" required
            variant="outlined" />

          <v-text-field v-model="senha" label="Senha*" prepend-inner-icon="mdi-lock" type="password" required
            variant="outlined" />

          <p v-if="mensagemErro" class="mensagem-erro">{{ mensagemErro }}</p>

          <!-- Checkbox ajustado visualmente -->
          <v-checkbox v-model="lembrarDeMim" label="Lembrar de mim" hide-details class="checkbox" density="compact" />

          <small class="texto-obrigatorio">*Informações obrigatórias</small>

          <v-btn class="btn-login" rounded @click="fazerLogin">
            Entrar
          </v-btn>

          <router-link to="/esqueci-minha-senha" class="link-senha">
            Esqueceu a senha?
          </router-link>

        </v-form>
      </v-col>
    </v-row>
  </v-container>
  <!-- Modal de carregamento -->
  <div v-if="carregando" class="modal-loading">
    <div class="modal-card">
      <h4>Login feito com sucesso!</h4>
      <p>Aguarde, você está sendo redirecionado...</p>
    </div>
  </div>

</template>

<script>
import axios from "axios";

export default {
  name: "UserLogin",
  data() {
    return {
      email: "",
      senha: "",
      lembrarDeMim: false,
      carregando: false,
      mensagemErro: ""
    };
  },
  methods: {
    async fazerLogin() {
      const payload = {
        email: this.email,
        senha: this.senha
      };

      try {
        const response = await axios.post("http://localhost:5054/api/usuario/login", payload);

        localStorage.setItem("usuarioEmail", this.email);
        localStorage.setItem("usuarioNome", response.data.nome);
        localStorage.setItem("usuarioCPF", response.data.cpf);
        localStorage.setItem("usuarioId", response.data.id);

        if (this.lembrarDeMim) {
          localStorage.setItem("lembrarEmail", this.email);
        } else {
          localStorage.removeItem("lembrarEmail");
        }

        // Ativa o modal de carregamento
        this.carregando = true;

        // Aguarda um segundo antes de redirecionar
        setTimeout(() => {
          this.$router.push("/home");
        }, 1000);
      } catch (err) {
        this.mensagemErro = "Usuário ou senha incorretos. Verifique suas informações.";
      }

    }
  },
  mounted() {
    const emailSalvo = localStorage.getItem("lembrarEmail");
    if (emailSalvo) {
      this.email = emailSalvo;
      this.lembrarDeMim = true;
    }
  },
  watch: {
    email() {
      this.mensagemErro = "";
    },
    senha() {
      this.mensagemErro = "";
    }
  }

};
</script>

<style scoped>
.login-container {
  height: 100vh;
  margin: 0;
  padding: 0;
  background-color: transparent;
}

.login-row {
  width: 100%;
  height: 100%;
}

.coluna-esquerda {
  background-color: #f46c20;
  color: white;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  height: 100vh;
}

.titulo-principal {
  font-size: 3.5rem;
  font-weight: bold;
}

.subtitulo {
  margin-top: 40px;
  font-weight: bold;
  font-size: 1.5rem;
}

.mensagem-criacao {
  text-align: center;
  width: 75%;
}

.btn-criar-conta {
  margin-top: 30px;
  color: #f46c20;
  background-color: white;
  font-weight: bold;
  padding: 10px 30px;
  box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.2);
}

.coluna-direita {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.mensagem-login {
  text-align: center;
  margin-bottom: 20px;
}

.formulario-login {
  width: 75%;
  margin-top: 20px;
}

.checkbox {
  margin-top: -20px;
  font-size: 14px;
  transform: scale(0.9);
  margin-left: -36px;
}

.texto-obrigatorio {
  display: block;
  color: #999;
  margin-bottom: 10px;
  font-size: 13px;
}

.btn-login {
  background-color: #f46c20;
  color: white;
  font-weight: bold;
  width: 100%;
  padding: 10px;
  box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.2);
}

.btn-login:hover {
  background-color: #d95c1d;
}

.link-senha {
  display: block;
  margin-top: 20px;
  text-align: center;
  color: #f46c20;
  font-weight: bold;
  text-decoration: none;
}

.modal-loading {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  /* fundo escurecido */
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.modal-card {
  background-color: #f46c20;
  /* laranja */
  border-radius: 16px;
  padding: 30px 40px;
  text-align: center;
  color: white;
  /* letras brancas */
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
  font-weight: bold;
  font-size: 1.1rem;
  max-width: 400px;
  width: 80%;
}

.mensagem-erro {
  color: red;
  font-weight: bold;
  margin: 0 0 55px 2px; /* Margem inferior para o próximo elemento (checkbox) */
  font-size: 0.9rem;
  display: block;
}


</style>