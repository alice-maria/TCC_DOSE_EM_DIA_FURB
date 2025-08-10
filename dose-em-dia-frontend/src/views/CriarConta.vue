<template>
  <div class="pagina">

    <header class="header" role="banner">
      <div class="logo-container" role="link" tabindex="0" aria-label="Ir para a página inicial"
        @click="$router.push('/')" @keydown.enter="$router.push('/')">
        <img src="@/imagens/logo.png" alt="Logo Dose em Dia" class="logo-img" />
        <div class="header-texts">
          <h1 class="titulo-principal">Crie sua conta</h1>
          <p class="subtitulo">Mantenha sua vacinação em dia!</p>
        </div>
      </div>
    </header>
    <!-- FUNDO LARANJA COM FORMULÁRIO -->
    <div class="container-laranja">
      <div class="formulario-caixa">

        <v-progress-linear :model-value="percentualPreenchido" color="#b2443a" height="6" rounded class="mb-6" />

        <v-form @submit.prevent="criarConta" ref="form">
          <!-- Abas -->
          <v-tabs v-model="abaAtiva" background-color="#fff4e1" grow>
            <v-tab :value="'pessoal'">Dados Pessoais</v-tab>
            <v-tab :value="'endereco'">Endereço</v-tab>
          </v-tabs>

          <v-window v-model="abaAtiva" class="mt-4">
            <!-- Aba Dados Pessoais -->
            <v-window-item value="pessoal">
              <v-container>
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

                <h3 class="seguranca">Segurança</h3>

                <v-text-field v-model="form.senha" label="Senha*" variant="outlined"
                  :type="mostrarSenha ? 'text' : 'password'" required @input="validarSenha"
                  :error="!senhaValida && form.senha.length > 0">
                  <template #append-inner>
                    <img :src="mostrarSenha ? iconeOlhoAberto : iconeOlhoFechado" class="icone-olho-custom"
                      @click.stop="mostrarSenha = !mostrarSenha" alt="Mostrar ou ocultar senha" />
                  </template>
                </v-text-field>

                <div class="mensagem-erro-wrapper">
                  <p v-if="form.senha.length > 0 && !senhaValida" class="text-danger-senha">
                    A senha deve conter no mínimo 8 caracteres e ao menos 1 letra maiúscula, 1 número e 1 caractere
                    especial.
                  </p>
                </div>

                <v-text-field v-model="form.confirmarSenha" label="Confirme sua senha*" variant="outlined"
                  :type="mostrarConfirmarSenha ? 'text' : 'password'" required
                  :error="form.confirmarSenha.length > 0 && form.confirmarSenha !== form.senha">
                  <template #append-inner>
                    <img :src="mostrarConfirmarSenha ? iconeOlhoAberto : iconeOlhoFechado" class="icone-olho-custom"
                      alt="Mostrar ou ocultar confirmação"
                      @click.stop="mostrarConfirmarSenha = !mostrarConfirmarSenha" />
                  </template>
                </v-text-field>

                <div class="mensagem-erro-wrapper">
                  <p v-if="form.confirmarSenha.length > 0 && form.confirmarSenha !== form.senha"
                    class="text-danger-senha">
                    As senhas não coincidem.
                  </p>
                </div>

                <!-- Checkbox ajustado visualmente -->
                <v-checkbox v-model="privacidade" density="compact" class="checkbox-privacidade"
                  :rules="[v => !!v || 'É necessário aceitar a política de privacidade.']">
                  <template #label>
                    <span>
                      Aceito as <a href="/aceitar-politica-privacidade" target="_blank">políticas de privacidade</a>
                    </span>
                  </template>
                </v-checkbox>

                <div class="d-flex justify-end mt-6">
                  <v-btn variant="outlined" color="orange" class="botao-japossuiconta me-3" @click="$router.push('/')">
                    <template #append>
                      <span class="caption d-block">Entrar! Já possuo uma conta</span>
                    </template>
                  </v-btn>
                  <v-btn variant="outlined" color="orange" class="botao-secundario" @click="abaAtiva = 'endereco'">
                    <template #append>
                      <span class="caption d-block">Avançar</span>
                    </template>
                  </v-btn>
                </div>

              </v-container>
            </v-window-item>

            <v-window-item value="endereco">
              <v-container>
                <v-text-field label="CEP*" v-model="form.cep" variant="outlined" v-mask="'#####-###'" @blur="buscarCep"
                  required />
                <v-text-field label="Endereço*" v-model="form.endereco.logradouro" variant="outlined" required />
                <v-text-field label="Estado*" v-model="form.endereco.estado" variant="outlined" required />
                <v-text-field label="País*" v-model="form.endereco.pais" variant="outlined" required />
                <v-text-field label="Cidade*" v-model="form.endereco.cidade" variant="outlined" required />
                <v-text-field label="Bairro*" v-model="form.endereco.bairro" variant="outlined" required />

                <!-- BOTÕES -->
                <div class="d-flex justify-end mt-6">
                  <v-btn variant="outlined" color="orange" class="botao-secundario me-3" @click="abaAtiva = 'pessoal'">
                    <template #append>
                      <span class="caption d-block">Voltar</span>
                    </template>
                  </v-btn>
                  <v-btn color="orange" class="botao-principal" type="submit">Criar </v-btn>
                </div>

              </v-container>
            </v-window-item>
          </v-window>

        </v-form>
        <small class="infos">*Informações obrigatórias</small>
      </div>
      <!-- Modal de carregamento -->
      <div v-if="carregando" class="modal-loading">
        <div class="modal-content text-white text-center p-4">
          <h4>Você está quase lá!</h4>
          <p>Criando conta...</p>
        </div>
      </div>
      <!-- Modal de sucesso -->
      <div v-if="modalSucesso" class="modal-loading">
        <div class="modal-content text-white text-center p-4">
          <h4>Conta criada com sucesso!</h4>
          <v-btn color="white" class="botao-redirecionamento" @click="fecharModalSucesso">OK</v-btn>
        </div>
      </div>
      <!--Erro-->
      <v-dialog v-model="mostrarErro" max-width="400">
        <v-card>
          <v-alert type="error" color="red-darken-2" icon="mdi-alert-circle" class="pa-5" border="start" elevation="2"
            title="Erro ao salvar">
            {{ erro }}
          </v-alert>
          <v-card-actions class="justify-end">
            <v-btn color="red-darken-2" variant="flat" @click="mostrarErro = false">OK</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
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
      privacidade: false,
      abaAtiva: 'pessoal',
      modalSucesso: false,
      erro: '',
      mostrarErro: false,
      iconeOlhoAberto: require('@/assets/icons/eyes-on.svg'),
      iconeOlhoFechado: require('@/assets/icons/eyes-off.svg'),
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

      const cpfLimpo = this.form.cpf.replace(/\D/g, "").padStart(11, "0"); 

      const payload = {
        nome: this.form.nome,
        email: this.form.email,
        telefone: this.form.telefone,
        cpf: cpfLimpo,
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
        const response = await axios.post("http://localhost:5054/api/usuario/criar-conta", payload);
        localStorage.setItem("usuarioNome", response.data.nome);
        localStorage.setItem("usuarioCpf", cpfLimpo);
        localStorage.setItem("usuarioId", response.data.idUser); 
        localStorage.setItem("usuarioEmail", response.data.email); 
        console.log("Usuário criado:", response.data);
        this.modalSucesso = true;
      } catch (err) {
        if (err.response && err.response.data) {
          console.error("Erro do servidor:", err.response.data);
          const erroMensagem = typeof err.response.data === 'string'
            ? err.response.data
            : JSON.stringify(err.response.data);
          this.erro = erroMensagem;
          this.mostrarErro = true;
        } else {
          console.error("Erro inesperado:", err);
          this.erro = "Erro desconhecido ao criar conta.";
          this.mostrarErro = true;
        }
      } finally {
        this.carregando = false;
      }
    },
    fecharModalSucesso() {
      this.modalSucesso = false;
      this.$router.push('/home');
    },
    fecharErro() {
      this.dialogErro = false;
    }
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

.header {
  background-color: #ffffff;
  border-bottom: 1px solid #eee;
  padding: 1.25rem 2rem; 
  min-height: 120px;     
  display: flex;
  align-items: center;
}

.logo-container {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  cursor: pointer;
  outline: none;
}

.logo-img {
  width: auto;
  object-fit: contain;
  height: 155px;
  margin-top: -30px;
}

.titulo-principal {
  font-weight: 800;
  color: #f46c20;
  margin: 0;
  line-height: 1.1;
  font-size: 1.5rem;
  margin-top: -18px;
}

.subtitulo {
  color: #6b7280;
  margin: 0.15rem 0 0 0;
  font-weight: 600;
  font-size: 0.95rem;
}

.container-laranja {
  background-color: #f46c20;
  display: flex;
  justify-content: center;
  align-items: start;
  margin-top: -1.5rem;
  padding: 2rem 1rem;
  overflow: hidden;
  border-top: 8px solid #f46c20;
}

.formulario-caixa {
  background-color: white;
  border-radius: 10px;
  max-width: 1640px;
  width: 102%;
  height: 108%;
  max-height: 85vh;
  padding: 2rem 4rem;
  overflow-y: auto;
  box-shadow: 0 6px 15px rgba(0, 0, 0, 0.1);
  margin-top: -30px;
}

.v-text-field,
.v-select {
  width: 100%;
}

.modal-loading {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
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
  background-color: #fff4e1;
}

.v-tabs {
  border-bottom: 1px solid #ccc;
}

.v-tab {
  color: #f46c20 !important;
  font-weight: bold;
}

.v-tab--selected {
  color: #f46c20 !important;
  border-bottom: 2px solid #f46c20 !important;
}

.seguranca {
  color: #f46c20;
}

.icone-olho {
  width: 24px;
  height: 24px;
  margin-top: -120px;
  margin-left: calc(100% - 36px);
}

.text-danger-senha {
  color: red;
  font-size: 0.9rem;
  font-weight: 500;
}

.mensagem-erro-wrapper {
  margin-top: -10px;
  margin-bottom: 24px;
}

.infos {
  margin-top: -1000px;
  margin-left: 0;
  color: #ccc;
}

.checkbox-privacidade {
  margin-top: -25px;
  font-size: 14px;
  margin-left: 0px;
}

.botao-principal,
.botao-redirecionamento,
.botao-secundario,
.botao-japossuiconta {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  border-radius: 30px;
  line-height: 1.2;
  padding: 12px 24px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.botao-principal {
  background-color: #f46c20 !important;
  color: white !important;
}

.botao-redirecionamento {
  background-color: white !important;
  color: #f46c20 !important;
}

.botao-secundario {
  border: 2px solid;
  color: darkgray !important;
  background-color: transparent !important;
}

.botao-japossuiconta {

  background-color: transparent !important;
}

.v-alert {
  font-size: 1rem;
}

.v-card-actions .v-btn {
  font-weight: normal;
  border-radius: 20px;
  padding: 6px 16px;
}
</style>
