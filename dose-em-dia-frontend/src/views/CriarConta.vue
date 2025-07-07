<template>
  <div class="pagina">
    <!-- TOPO BRANCO COM TÍTULOS -->
    <div class="cabecalho-branco">
      <h1 class="titulo-principal" @click="$router.push('/')">Dose em dia </h1>
      <h2 class="segundo-titulo">Crie sua conta</h2>
      <p class="terceiro-titulo">Mantenha sua vacinação em dia!</p>
    </div>

    <!-- FUNDO LARANJA COM FORMULÁRIO -->
    <div class="container-laranja">
      <div class="formulario-caixa">

        <v-progress-linear :model-value="percentualPreenchido" color="#b2443a" height="6" rounded
          class="mb-6" />

        <v-form @submit.prevent="criarConta" ref="form">
          <v-row dense>
            <!-- Coluna 1 -->
            <v-col cols="12" md="6">
              <v-text-field label="Nome completo*" v-model="form.nome" variant="outlined" required />
              <v-text-field label="E-mail*" v-model="form.email" variant="outlined" type="email" @input="validarEmail"
                :class="{
                  'is-valid': emailValido,
                  'is-invalid': !emailValido && form.email.length > 0
                }" required />
              <p v-if="form.email.length > 0" :class="{ 'text-success': emailValido, 'text-danger': !emailValido }">
                {{ emailValido ? 'E-mail válido' : 'E-mail inválido' }}
              </p>

              <v-text-field label="Telefone*" v-model="form.telefone" variant="outlined" v-mask="'(##) #####-####'"
                required />
              <v-text-field label="CPF*" v-model="form.cpf" variant="outlined" v-mask="'###.###.###-##'" required />
              <v-text-field label="Data de nascimento*" v-model="form.dataNascimento" variant="outlined" type="date"
                required />
              <v-select label="Sexo*" v-model="form.sexo" variant="outlined"
                :items="['Masculino', 'Feminino', 'Não informar']" required />
            </v-col>

            <!-- Coluna 2 -->
            <v-col cols="12" md="6">
              <v-text-field label="CEP*" v-model="form.cep" variant="outlined" v-mask="'#####-###'" @blur="buscarCep"
                required />
              <v-text-field label="Endereço*" v-model="form.endereco.logradouro" variant="outlined" required />
              <v-text-field label="Estado*" v-model="form.endereco.estado" variant="outlined" required />
              <v-text-field label="País*" v-model="form.endereco.pais" variant="outlined" required />
              <v-text-field label="Cidade*" v-model="form.endereco.cidade" variant="outlined" required />
              <v-text-field label="Bairro*" v-model="form.endereco.bairro" variant="outlined" required />
              <v-text-field label="Senha*" v-model="form.senha" variant="outlined"
                :type="mostrarSenha ? 'text' : 'password'" :append-inner-icon="mostrarSenha ? 'mdi-eye' : 'mdi-eye-off'"
                @click:append-inner="mostrarSenha = !mostrarSenha" @input="validarSenha"
                :class="{ 'is-valid': senhaValida, 'is-invalid': erroSenha && form.senha.length > 0 }" required />
              <p v-if="!senhaValida && form.senha.length > 0" class="text-danger mt-1">
                A senha deve conter no mínimo 8 caracteres e ao menos 1 letra maiúscula, 1 número e 1 caractere
                especial.
              </p>

              <v-text-field label="Confirme sua senha*" v-model="form.confirmarSenha" variant="outlined"
                :type="mostrarConfirmarSenha ? 'text' : 'password'"
                :append-inner-icon="mostrarConfirmarSenha ? 'mdi-eye' : 'mdi-eye-off'"
                @click:append-inner="mostrarConfirmarSenha = !mostrarConfirmarSenha"
                :class="{ 'is-invalid': form.confirmarSenha.length > 0 && form.confirmarSenha !== form.senha }"
                required />

              <p v-if="form.confirmarSenha.length > 0 && form.confirmarSenha !== form.senha" class="text-danger mt-1">As
                senhas não coincidem.</p>
            </v-col>
          </v-row>
          <!-- BOTÕES -->
          <div class="d-flex justify-end mt-6">
            <v-btn variant="outlined" color="orange" class="botao-secundario me-3" @click="$router.push('/')">
              <template #append>
                <span class="caption d-block">Voltar</span>
              </template>
            </v-btn>
            <v-btn color="orange" class="botao-principal" type="submit">Criar </v-btn>
          </div>
        </v-form>
        <small class="text-muted mt-2 d-block">*Informações obrigatórias</small>
      </div>
      <!-- Modal de carregamento -->
      <div v-if="carregando" class="modal-loading">
        <div class="modal-content text-white text-center p-4">
          <h4>Você está quase lá!</h4>
          <p>Criando conta...</p>
          <button class="btn btn-light mt-3" @click="carregando = false">Cancelar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import { mask } from "vue-the-mask";

export default {
  name: "CriarConta",
  directives: { mask },
  data() {
    return {
      carregando: false,
      mostrarSenha: false,
      mostrarConfirmarSenha: false,
      emailValido: false,
      senhaValida: false,
      erroSenha: false,
      form: {
        nome: "",
        email: "",
        telefone: "",
        cpf: "",
        dataNascimento: "",
        sexo: "",
        cep: "",
        senha: "",
        confirmarSenha: "",
        endereco: {
          logradouro: "",
          bairro: "",
          cidade: "",
          estado: "",
          cep: "",
          pais: "",
        },
      },
    };
  },
  computed: {
    percentualPreenchido() {
      const campos = [
        this.form.nome,
        this.form.email,
        this.form.telefone,
        this.form.cpf,
        this.form.dataNascimento,
        this.form.sexo,
        this.form.cep,
        this.form.endereco.logradouro,
        this.form.endereco.estado,
        this.form.endereco.pais,
        this.form.endereco.cidade,
        this.form.endereco.bairro,
        this.form.senha,
        this.form.confirmarSenha
      ];
      const preenchidos = campos.filter(c => c && c.toString().trim() !== '').length;
      return Math.round((preenchidos / campos.length) * 100);
    }
  },
  methods: {
    validarEmail() {
      const field = this.form.email;
      const usuario = field.substring(0, field.indexOf("@"));
      const dominio = field.substring(field.indexOf("@") + 1, field.length);

      this.emailValido = (
        usuario.length >= 1 &&
        dominio.length >= 3 &&
        usuario.indexOf("@") === -1 &&
        dominio.indexOf("@") === -1 &&
        usuario.indexOf(" ") === -1 &&
        dominio.indexOf(" ") === -1 &&
        dominio.indexOf(".") !== -1 &&
        dominio.indexOf(".") >= 1 &&
        dominio.lastIndexOf(".") < dominio.length - 1
      );

      console.log("E-mail:", field);
      console.log("E-mail Válido:", this.emailValido);
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

    async buscarCep() {
      const cepLimpo = this.form.cep.replace(/\D/g, "");
      if (cepLimpo.length !== 8) return;

      try {
        const response = await axios.get(
          `https://viacep.com.br/ws/${cepLimpo}/json/`
        );
        const data = response.data;
        if (data.erro) throw new Error("CEP inválido");

        this.form.endereco.logradouro = data.logradouro;
        this.form.endereco.bairro = data.bairro;
        this.form.endereco.cidade = data.localidade;
        this.form.endereco.estado = data.uf; //correção do BUG que ocorria ao tentar salvar um estado como "Sao paulo", limite de caracteres do banco era em 2
      } catch (error) {
        alert("CEP inválido ou erro ao buscar endereço.");
      }
    },
    async criarConta() {
      if (this.form.senha !== this.form.confirmarSenha) {
        alert("As senhas não coincidem.");
        return;
      }

      const payload = {
        nome: this.form.nome,
        email: this.form.email,
        telefone: this.form.telefone,
        cpf: this.form.cpf,
        dataNascimento: this.form.dataNascimento,
        sexo: this.form.sexo,
        senha: this.form.senha,
        endereco: {
          logradouro: this.form.endereco.logradouro,
          bairro: this.form.endereco.bairro,
          cidade: this.form.endereco.cidade,
          estado: this.form.endereco.estado,
          cep: this.form.cep,
          pais: this.form.endereco.pais
        },
      };

      try {
        this.carregando = true;
        const response = await axios.post("http://localhost:5054/api/usuario/criar", payload);
        localStorage.setItem("usuarioNome", response.data.nome);
        console.log("Usuário criado:", response.data);
        alert("Conta criada com sucesso!");
        this.$router.push("/");
      } catch (err) {
        // 👇 Novo tratamento de erro com mais clareza
        if (err.response && err.response.data) {
          console.error("Erro do servidor:", err.response.data);
          const erroMensagem = typeof err.response.data === 'string'
            ? err.response.data
            : JSON.stringify(err.response.data);
          alert("Erro ao criar conta: " + erroMensagem);
        } else {
          console.error("Erro inesperado:", err);
          alert("Erro desconhecido ao criar conta.");
        }
      } finally {
        this.carregando = false;
      }

    },
  },
};
</script>

<style scoped>
.pagina {
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow: hidden;
}

.cabecalho-branco {
  background-color: white;
  border-bottom: none;
  padding: 2rem 2.5rem 1rem;
  text-align: left;
}

.titulo-principal {
  font-size: 40px;
  font-weight: 900;
  color: #f46c20;
  margin-bottom: 0.5rem;
  margin-top: -1.5rem;
}

/* Container laranja ocupa o restante da tela */
.container-laranja {
  background-color: #f46c20;
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: start;
  margin-top: -1.5rem;
  padding: 2rem 1rem;
  overflow: hidden;
  border-top: 8px solid #f46c20;
  /* linha laranja contínua, opcional */
}

/* Caixa branca com scroll interno se necessário */
.formulario-caixa {
  background-color: white;
  border-radius: 20px;
  max-width: 1640px;
  width: 100%;
  height: 100%;
  max-height: 85vh;
  padding: 2rem 4rem;
  overflow-y: auto;
  box-shadow: 0 6px 15px rgba(0, 0, 0, 0.1);
}

/* Aumentar largura dos campos */
.v-text-field,
.v-select {
  width: 100%;
}

.botao-principal {
  background-color: #f46c20 !important;
  color: white !important;
  font-weight: bold;
  border-radius: 30px;
  padding: 10px 24px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.botao-secundario {
  border: 2px solid;
  color: darkgray !important;
  border-radius: 30px;
}

.segundo-titulo {
  font-size: 23px;
  color: darkgray;
  margin-top: -0.5rem;
}

.terceiro-titulo {
  font-size: 18px;
  color: darkgray;
  margin-top: -0.7rem;
}

.modal-loading {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  /* fundo escuro translúcido */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.modal-content {
  background-color: #f46c20;
  border-radius: 20px;
  padding: 40px;
  width: 300px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
}
.v-progress-linear {
  background-color: #fff4e1; /* lilás bem claro */
}

</style>
