<template>
  <v-app>
    <v-main>
      <v-container class="fill-height" fluid>
        <v-row align="center" justify="center">
          <v-col cols="12" sm="8" md="4">
            <v-card class="pa-6" elevation="8">
              <v-card-title class="text-orange text-h5 font-weight-bold">Esqueci minha senha</v-card-title>
              <v-card-text class="text-body-2 mb-4">Informe seu e-mail para receber um link de redefinição de
                senha.</v-card-text>
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
              <v-alert v-if="mensagem" type="error" color="red-darken-2" class="mt-4" border="start" elevation="2"
                icon="mdi-alert-circle">
                {{ mensagem }}
              </v-alert>
            </v-card>
          </v-col>
        </v-row>

        <!-- POPUP de confirmação -->
        <v-dialog v-model="dialogSucesso" max-width="420" persistent>
          <v-card>
            <v-card-title class="text-h6 font-weight-bold">
              Solicitação enviada
            </v-card-title>
            <v-card-text class="text-body-2">
              Se o e-mail estiver cadastrado, um link foi enviado para redefinir sua senha.
              Verifique sua caixa de entrada e o spam.
            </v-card-text>
            <v-card-actions class="justify-end">
              <v-btn color="orange" variant="flat" class="rounded-pill" @click="fecharDialog">
                Ok
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-container>
    </v-main>
  </v-app>
</template>

<script>
import axios from 'axios';

const baseURL = process.env.VUE_APP_API_BASE_URL || "https://doseemdiabackend-production.up.railway.app";

export const api = axios.create({
  baseURL: baseURL.replace(/\/+$/, ""),
  timeout: 20000,
});

export default {
  name: 'EsqueciMinhaSenha',
  data() {
    return {
      email: '',
      mensagem: '',
      dialogSucesso: false,   
      carregando: false,     
    };
  },
  methods: {
    async enviarEmail() {
      this.mensagem = '';
      this.carregando = true;
      try {
        await api.post('/api/usuario/esqueciSenha', { email: this.email });
        this.dialogSucesso = true;
      } catch (err) {
        this.mensagem = err.response?.data || 'Erro ao solicitar redefinição.';
      }
      finally {
        this.carregando = false;
      }
    },
    cancelar() {
      this.$router.push('/');
    },fecharDialog() {
      this.dialogSucesso = false;
    },
  }
};
</script>

<style scoped>
.text-orange {
  color: #f46c20;
}
</style>