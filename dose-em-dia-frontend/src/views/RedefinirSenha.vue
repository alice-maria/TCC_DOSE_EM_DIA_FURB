<template>
    <div class="container d-flex justify-content-center align-items-center vh-100">
      <div class="card p-4 shadow w-100" style="max-width: 500px;">
        <h3 class="text-orange fw-bold mb-3">Redefinir senha</h3>
        <form @submit.prevent="redefinirSenha">
          <div class="mb-3">
            <label for="novaSenha" class="form-label">Nova senha</label>
            <input
              type="password"
              id="novaSenha"
              v-model="novaSenha"
              class="form-control"
              required
            />
          </div>
          <button type="submit" class="btn btn-orange w-100">Redefinir</button>
        </form>
        <div v-if="mensagem" class="alert alert-info mt-3">{{ mensagem }}</div>
      </div>
    </div>
  </template>
  
  <script>
  import axios from 'axios';
  
  export default {
    name: 'RedefinirSenha',
    data() {
      return {
        novaSenha: '',
        mensagem: ''
      };
    },
    methods: {
      async redefinirSenha() {
        const token = new URLSearchParams(window.location.search).get('token');
        if (!token) {
          this.mensagem = 'Token inválido ou ausente.';
          return;
        }
  
        try {
          await axios.post('http://localhost:5054/api/usuario/redefinir-senha', {
            token: token,
            novaSenha: this.novaSenha
          });
          this.mensagem = 'Senha redefinida com sucesso. Você pode fazer login agora.';
          setTimeout(() => this.$router.push('/'), 3000);
        } catch (err) {
          this.mensagem = err.response?.data || 'Erro ao redefinir senha.';
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
  