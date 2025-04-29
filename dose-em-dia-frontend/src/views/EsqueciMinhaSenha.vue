<template>
    <div class="container d-flex justify-content-center align-items-center vh-100">
      <div class="card p-4 shadow w-100" style="max-width: 500px;">
        <h3 class="text-orange fw-bold mb-3">Esqueci minha senha</h3>
        <p class="text-muted">Informe seu e-mail para receber um link de redefinição de senha.</p>
        <form @submit.prevent="enviarEmail">
          <div class="mb-3">
            <label for="email" class="form-label">E-mail</label>
            <input
              type="email"
              id="email"
              v-model="email"
              class="form-control"
              required
            />
          </div>
          <button type="submit" class="btn btn-orange w-100">Enviar link de redefinição</button>
        </form>
        <div v-if="mensagem" class="alert alert-info mt-3">{{ mensagem }}</div>
      </div>
    </div>
  </template>
  
  <script>
  import axios from 'axios';
  
  export default {
    name: 'EsqueciMinhaSenha',
    data() {
      return {
        email: '',
        mensagem: ''
      };
    },
    methods: {
      async enviarEmail() {
        try {
          await axios.post('http://localhost:5054/api/usuario/esqueciSenha', { email: this.email });
          this.mensagem = 'Se o e-mail estiver cadastrado, um link foi enviado para redefinir sua senha.';
        } catch (err) {
          this.mensagem = err.response?.data || 'Erro ao solicitar redefinição.';
        }
      }
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
    border: none;
  }
  .btn-orange:hover {
    background-color: #d85c1a;
  }
  </style>
  