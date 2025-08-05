<template>
  <v-container fluid class="pa-0">
    <div class="pagina-edituser">
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
            <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt="" class="breadcrumb-home-img" />
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <p class="px-5 subtitulo">Informações sobre seu perfil e dados cadastrais.</p>

      <div class="linha-dado">
        <label>Nome:</label>
        <div>
          <span v-if="!editando">{{ usuario.nome }}</span>
          <input v-else v-model="form.nome" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>E-mail:</label>
        <div>
          <span v-if="!editando">{{ usuario.email }}</span>
          <input v-else v-model="form.email" type="email" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Telefone:</label>
        <div>
          <span v-if="!editando">{{ formatarTelefone(usuario.telefone) }}</span>
          <input v-else v-mask="'(##) #####-####'" v-model="form.telefone" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>CPF:</label>
        <div>
          <span v-if="!editando">{{ formatarCPF(usuario.cpf) }}</span>
          <input v-else v-model="form.cpf" type="text" class="form-control" disabled>
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Data de Nascimento:</label>
        <div>
          <span v-if="!editando">{{ formatarData(usuario.dataNascimento) }}</span>
          <input v-else v-model="form.dataNascimento" type="date" class="form-control" disabled>
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Sexo:</label>
        <div>
          <span v-if="!editando">{{ usuario.sexo || 'Não informado' }}</span>
          <input v-else v-model="form.sexo" type="text" class="form-control" disabled>
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>CEP:</label>
        <div>
          <span v-if="!editando">{{ form.endereco.cep || 'Não informado' }}</span>
          <input v-else v-mask="'#####-###'" v-model="form.endereco.cep" type="text" class="form-control"
            @blur="buscarCep">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Endereço:</label>
        <div>
          <span v-if="!editando">{{ usuario.endereco?.logradouro || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.logradouro" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Bairro:</label>
        <div>
          <span v-if="!editando">{{ usuario.endereco?.bairro || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.bairro" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Cidade:</label>
        <div>
          <span v-if="!editando">{{ usuario.endereco?.cidade || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.cidade" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Estado:</label>
        <div>
          <span v-if="!editando">{{ usuario.endereco?.estado || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.estado" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="text-center mt-4">
        <v-btn v-if="!editando" color="orange" class="botao-preenchido" @click="editando = true">
          Editar dados
        </v-btn>

        <div v-else class="d-flex justify-content-center gap-3">
          <v-btn class="botao-tonal" @click="cancelar">
            Cancelar
          </v-btn>
          <v-btn color="orange" class="botao-preenchido" @click="confirmarSalvar = true">
            Salvar
          </v-btn>
        </div>
      </div>
    </div>
  </v-container>
  <v-dialog v-model="confirmarSalvar" max-width="400" persistent>
    <v-card class="popup-confirmacao">
      <v-card-text class="texto-confirmacao">
        Você confirma a alteração dos dados?
      </v-card-text>
      <div class="botoes-popup">
        <v-btn class="btn-cancelar" @click="confirmarSalvar = false">Cancelar</v-btn>
        <v-btn class="btn-confirmar" @click="confirmarSalvar = false; salvar()">Confirmar</v-btn>
      </div>
    </v-card>
  </v-dialog>

</template>

<script>
import axios from 'axios';

export default {
  name: 'EdicaoUsuario',
  data() {
    return {
      usuario: {},
      form: {
        nome: '',
        email: '',
        telefone: '',
        cpf: '',
        dataNascimento: '',
        sexo: '',
        cep: "",
        endereco: {
          logradouro: "",
          bairro: "",
          cidade: "",
          estado: "",
        }
      },
      editando: false,
      erro: '',
      mostrarErro: false,
      confirmarSalvar: false,
      breadcrumbs: [
        { text: 'Serviços e Informações', to: '/home', icon: 'mdi-home' },
        { text: 'Configurações', to: '/configuracoes' },
        { text: 'Informações Cadastrais' }
      ]
    };
  },
  methods: {
    navegar(rota) {
      if (rota) {
        this.$router.push(rota);
      }
    },

    formatarData(data) {
      return new Date(data).toLocaleDateString('pt-BR');
    },
    async carregarUsuario() {
      const cpf = localStorage.getItem("usuarioCPF");
      if (!cpf) return;
      try {
        const response = await axios.get(`http://localhost:5054/api/usuario/buscarPorCpf/${cpf}`);
        this.usuario = response.data;
        const dataFormatada = response.data.dataNascimento?.split('T')[0];
        this.form = {
          ...response.data,
          dataNascimento: dataFormatada,
          endereco: response.data.endereco || { logradouro: '', cidade: '' }
        };
      } catch (error) {
        this.erro = 'Erro ao carregar dados do usuário.';
      }
    },
    async salvar() {
      try {
        await axios.patch(`http://localhost:5054/api/usuario/alterarDados/${this.usuario.idUser}`, this.form);
        this.editando = false;
        await this.carregarUsuario();
      } catch (error) {
        console.error(error);
        if (error.response?.data?.message) {
          this.erro = error.response.data.message;
        } else if (typeof error === 'string') {
          this.erro = error;
        } else {
          this.erro = 'Erro ao salvar os dados. Tente novamente.';
        }
        this.mostrarErro = true;
      }
    },
    cancelar() {
      this.editando = false;
      this.carregarUsuario();
    },
    formatarTelefone(telefone) {
      if (!telefone) return '';
      const numeros = telefone.replace(/\D/g, '');
      if (numeros.length === 11)
        return `(${numeros.slice(0, 2)}) ${numeros.slice(2, 7)}-${numeros.slice(7)}`;
      if (numeros.length === 10)
        return `(${numeros.slice(0, 2)}) ${numeros.slice(2, 6)}-${numeros.slice(6)}`;
      return telefone;
    },

    formatarCPF(cpf) {
      if (!cpf) return '';
      const numeros = cpf.replace(/\D/g, '');
      return numeros.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    },
    async buscarCep() {
      const cepLimpo = this.form.endereco.cep?.replace(/\D/g, "");
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
  },
  mounted() {
    this.carregarUsuario();
    this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
  }
};
</script>

<style scoped>
.pagina-edituser {
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
  transition: color 0.2s;
  font-size: 1.1rem;
}

.breadcrumb-link:hover {
  color: #f97316;
  text-decoration: underline;
}

.breadcrumb-home-img {
    margin-top: -5px;
}

.breadcrumb-laranja {
  color: #f97316 !important;
  font-weight: 900;
  font-size: 1.1rem;
}

.subtitulo {
  margin-top: -10px;
  font-size: 15px;
}

.linha-dado {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  font-size: 1rem;
  padding: 0.9rem;
  margin-left: 8px;
}

.linha-dado label {
  font-weight: bold;
  color: #000;
  flex: 1;
  min-width: 140px;
  margin: 0;
}

.linha-dado div {
  flex: 2;
  text-align: left;
  color: #333;
}

.v-alert {
  font-size: 1rem;
}

.v-card-actions .v-btn {
  font-weight: normal;
  border-radius: 20px;
  padding: 6px 16px;
}

.btn-orange,
.botao-preenchido,
.botao-tonal {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  line-height: 1.2;
  font-weight: bold;
  padding: 12px 24px;
  border-radius: 24px;
  text-transform: none;
}

.btn-orange {
  background-color: #f46c20;
  color: white;
  border: none;
}

.botao-preenchido {
  background-color: #f46c20 !important;
  color: white !important;
  box-shadow: 0px 3px 6px rgba(0, 0, 0, 0.15);
  margin-top: -50px;
}

.botao-tonal {
  background-color: #e0e0e0 !important;
  color: #212121 !important;
  font-weight: 600;
  margin-top: -50px;
}

.linha-dado input.form-control {
  height: 50px;
  font-size: 1.0rem;
  padding: 12px 16px;
  line-height: 1.5;
  border: 1.5px solid #ccc;
  margin-left: -90px;
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
</style>