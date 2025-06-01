<template>
  <div class="container py-5">
    <h2 class="text-orange fw-bold mb-3">Informações cadastrais</h2>
    <p class="text-muted mb-4">Informações sobre seu perfil e dados cadastrais.</p>

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
      erro: null
    };
  },
  methods: {
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
  }
};
</script>

<style scoped>
.text-orange {
  color: #f46c20;
}
.btn-orange {
  background-color: #f46c20;
  color: white;
  font-weight: bold;
  border: none;
}
</style>