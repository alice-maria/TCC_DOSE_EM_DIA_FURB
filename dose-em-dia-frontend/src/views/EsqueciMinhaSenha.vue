<template>
  <v-app>
    <v-main>
      <v-container class="fill-height" fluid>
        <v-row align="center" justify="center">
          <v-col cols="12" sm="8" md="4">
            <v-card class="pa-6" elevation="8">
              <!-- Título -->
              <v-card-title class="text-orange text-h5 font-weight-bold">Esqueci minha senha</v-card-title>
              <!-- Descrição -->
              <v-card-text class="text-body-2 mb-4">Informe seu e-mail para receber um link de redefinição de
                senha.</v-card-text>
              <!-- Formulário -->
              <v-form @submit.prevent="enviarEmail">
                <v-text-field v-model="email" label="E-mail" type="email" variant="outlined" required class="mb-4" />
                <!-- Botões lado a lado -->
                <v-row dense>
                  <v-col cols="6">
                    <v-btn type="submit" color="orange" class="rounded-pill py-3 text-body-1" block>Enviar</v-btn>
                  </v-col>
                  <v-col cols="6">
                    <v-btn variant="outlined" color="grey" class="rounded-pill py-3 text-body-1" block
                      @click="cancelar">Cancelar</v-btn>
                  </v-col>
                </v-row>
              </v-form>

              <!-- Mensagem -->
              <v-alert v-if="mensagem" type="info" class="mt-4">{{ mensagem }}</v-alert>
            </v-card>
          </v-col>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
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
    },
    cancelar() {
      this.$router.push('/');
    }

  }
};
</script>

<style scoped>
.text-orange {
  color: #f46c20;
}
</style>