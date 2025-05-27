<template>
  <div
    class="container-fluid vh-100 d-flex align-items-center justify-content-center"
  >
    >
    <div class="row w-100 h-100">
      <!-- Coluna Esquerda -->
      <div
        class="col-md-6 d-flex flex-column justify-content-center align-items-center bg-orange text-white p-5"
      >
        <h1 class="display-4 fw-bold">Dose em dia</h1>
        <h3 class="mt-5">Bem vindo!</h3>
        <p class="text-center w-75">
          Proteja sua saúde! Crie sua conta e mantenha sua vacinação em dia.
        </p>
        <router-link
          to="/criar-conta"
          class="btn btn-light text-orange fw-bold px-4 py-2 rounded-pill shadow mt-3 text-decoration-none"
        >
          Criar conta
        </router-link>
      </div>

      <!-- Coluna Direita -->
      <div
        class="col-md-6 d-flex flex-column justify-content-center align-items-center p-5"
      >
        <h3 class="fw-bold">Bem vindo de volta!</h3>
        <p class="text-center">
          Para conectar conosco, por favor faça login com suas informações.
        </p>

        <form class="w-75 mt-3">
          <div class="mb-3">
            <label for="email" class="form-label visually-hidden">Email</label>
            <div class="input-group">
              <span class="input-group-text"
                ><i class="bi bi-envelope"></i
              ></span>
              <input
                type="email"
                id="email"
                class="form-control"
                placeholder="E-mail*"
                required
                v-model="email"
              />
            </div>
          </div>

          <div class="mb-3">
            <label for="senha" class="form-label visually-hidden">Senha</label>
            <div class="input-group">
              <span class="input-group-text"><i class="bi bi-lock"></i></span>
              <input
              type="password"
              id="senha"
              class="form-control"
              placeholder="Senha*"
              required
              v-model="senha"
              />
            </div>
          </div>

          <div class="form-check mb-3">
            <input class="form-check-input" type="checkbox" id="lembrar" />
            <label class="form-check-label" for="lembrar">Lembrar de mim</label>
          </div>

          <small class="d-block text-muted mb-3"
            >*Informações obrigatórias</small
          >

          <button
            type="button"
            class="btn btn-orange text-white fw-bold px-4 py-2 rounded-pill shadow w-100"
            @click="fazerLogin"
          >
            Entrar
          </button>

          <router-link to="/esqueci-minha-senha" class="text-orange fw-bold text-decoration-none">
            Esqueceu a senha?
          </router-link>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  // eslint-disable-next-line vue/multi-word-component-names
  name: "Login",
  data() {
    return {
      email: "",
      senha: ""
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
        localStorage.setItem("usuarioNome", response.data.nome); //salvar nome do usuario

        alert("Login feito com sucesso!");

        //armazena e-mail e nome no localStorage
        localStorage.setItem("usuarioEmail", this.email);
        localStorage.setItem("usuarioNome", response.data.nome);
        localStorage.setItem("usuarioCPF", response.data.cpf); // adiciona o CPF pra validação de vacinas
        localStorage.setItem("usuarioId", response.data.id); //salva o id do usuário pra puxar comprovante no local storage, motivo de bug no primeiro teste

        // Redirecionar para a rota da tela inicial:
        this.$router.push("/home");
        console.log("login teste:", response.data);
      } catch (err) {
        alert("Erro ao fazer login: " + (err.response?.data || err.message));
      }
    }
  }
};
</script>

<style scoped>
.bg-orange {
  background-color: #f46c20;
}

.text-orange {
  color: #f46c20;
}

.btn-orange {
  background-color: #f46c20;
  border: none;
}

.btn-orange:hover {
  background-color: #d95c1d;
}
</style>
