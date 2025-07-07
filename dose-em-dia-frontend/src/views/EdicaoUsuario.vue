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
            <v-icon left small v-if="item.icon">{{ item.icon }}</v-icon>
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <p class="text-muted mb-4 px-6 subtitulo">Informações sobre seu perfil e dados cadastrais.</p>

      <div v-if="erro" class="alert alert-danger">{{ erro }}</div>

      <div class="card p-4 shadow-sm">
        <div class="row mb-3">
          <label class="fw-bold col-3">Nome:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.nome }}</span>
            <input v-else v-model="form.nome" type="text" class="form-control" disabled>
          </div>
        </div>

        <div class="row mb-3">
          <label class="fw-bold col-3">E-mail:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.email }}</span>
            <input v-else v-model="form.email" type="email" class="form-control">
          </div>
        </div>

        <div class="row mb-3">
          <label class="fw-bold col-3">Telefone:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.telefone }}</span>
            <input v-else v-mask="'(##) #####-####'" v-model="form.telefone" type="text" class="form-control">
          </div>
        </div>

        <div class="row mb-3">
          <label class="fw-bold col-3">CPF:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.cpf }}</span>
            <input v-else v-model="form.cpf" type="text" class="form-control" disabled>
          </div>
        </div>

        <div class="row mb-3">
          <label class="fw-bold col-3">Data de Nascimento:</label>
          <div class="col-9">
            <span v-if="!editando">{{ formatarData(usuario.dataNascimento) }}</span>
            <input v-else v-model="form.dataNascimento" type="date" class="form-control" disabled>
          </div>
        </div>

        <div class="row mb-3">
          <label class="fw-bold col-3">Sexo:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.sexo || 'Não informado' }}</span>
            <input v-else v-model="form.sexo" type="text" class="form-control" disabled>
          </div>
        </div>

        <div class="row mb-3">
          <label class="fw-bold col-3">Endereço:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.endereco?.logradouro || 'Não informado' }}</span>
            <input v-else v-model="form.endereco.logradouro" type="text" class="form-control">
          </div>
        </div>

        <div class="row mb-4">
          <label class="fw-bold col-3">Cidade:</label>
          <div class="col-9">
            <span v-if="!editando">{{ usuario.endereco?.cidade || 'Não informado' }}</span>
            <input v-else v-model="form.endereco.cidade" type="text" class="form-control">
          </div>
        </div>

        <div class="text-center">
          <button v-if="!editando" @click="editando = true" class="btn btn-orange px-4">Editar dados</button>
          <div v-else>
            <button @click="salvar" class="btn btn-success me-2">Salvar</button>
            <button @click="cancelar" class="btn btn-danger">Cancelar</button>
          </div>
        </div>
      </div>
    </div>
  </v-container>
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
        endereco: {
          logradouro: '',
          cidade: ''
        }
      },
      editando: false,
      erro: null,
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

        // Extrai só a parte "YYYY-MM-DD" da data para o input
        const dataFormatada = response.data.dataNascimento?.split('T')[0];

        this.form = {
          ...response.data,
          dataNascimento: dataFormatada, // <- aqui o ajuste
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
        this.erro = 'Erro ao salvar os dados.';
      }
    },
    cancelar() {
      this.editando = false;
      this.carregarUsuario();
    }
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
  color: #6b7280;                 /* cinza padrão */
  transition: color 0.2s;
  font-size: 1.1rem;
}

.breadcrumb-link:hover {
  color: #f97316;                 /* hover laranja */
  text-decoration: underline;
}

.breadcrumb-laranja {
  color: #f97316 !important;      /* laranja fixo no último */
  font-weight: 900;
  font-size: 1.1rem;
}

.btn-orange {
  background-color: #f46c20;
  color: white;
  font-weight: bold;
  border: none;
}

.subtitulo {
margin-top: -10px;
font-size: 18px;
}
</style>