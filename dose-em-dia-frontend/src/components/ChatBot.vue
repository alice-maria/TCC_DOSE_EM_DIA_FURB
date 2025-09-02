<template>
  <div>
    <button class="chatbot-toggle" @click="alternarChat" ref="botao" aria-label="Abrir chatbot">
      <img src="@/imagens/ChatBotVitta.png" alt="Abrir chatbot" />
    </button>
    <div v-show="visivel" ref="chat" class="chatbot-popup chatbot" role="dialog" aria-modal="true">
      <transition name="chat-fade">
        <div v-if="visivel">
          <button class="fechar-chat" @click="fecharChat" aria-label="Fechar chatbot">
            <img src="@/assets/icons/Icone-fechar-chatbot.svg" alt="Fechar" />
          </button>

          <div v-if="estado === 'boasVindas' || estado === 'inicio'">
            <div class="mensagem bot">
              <div class="saudacao">Olá, {{ nomeUsuario }}!</div>
              <p>Eu sou a Vitta, sua assistente virtual. Posso ajudar com vacinas, informações de saúde, sua conta e
                suporte técnico.</p>
              <p>Como posso te ajudar hoje?</p>
            </div>

            <div v-for="(opcao, index) in opcoes" :key="index" class="opcao">
              <button @click="executarAcao(opcao)">{{ opcao.label }}</button>
            </div>
          </div>
          <div v-else-if="estado === 'vacinas'">
            <div class="mensagem bot">O que você quer saber mais sobre suas vacinas?</div>

            <div v-for="(item, index) in submenuVacinas" :key="index" class="opcao">
              <button @click="executarAcaoDireta(item.acao)">{{ item.label }}</button>
            </div>

            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>
          <div v-else-if="estado === 'conta'">
            <div class="mensagem bot">O que deseja fazer em sua conta?</div>

            <div v-for="(item, index) in submenuConta" :key="index" class="opcao">
              <button @click="executarAcaoDireta(item.acao)">{{ item.label }}</button>
            </div>

            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>
          <div v-else-if="estado === 'educacao'">
            <div class="mensagem bot bolha-titulo">Sobre qual tema de saúde você quer saber mais?</div>

            <div class="grid-educacao">
              <button class="card-atalho" @click="estado = 'educacaoCrianca'">Bebês e crianças</button>
              <button class="card-atalho" @click="estado = 'educacaoAdolescente'">Adolescentes e jovens</button>
              <button class="card-atalho" @click="estado = 'educacaoAdulto'">Adultos</button>
              <button class="card-atalho" @click="estado = 'educacaoIdoso'">Idosos</button>
              <button class="card-atalho" @click="estado = 'educacaoGestante'">Gestantes</button>
            </div>

            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>
          <div v-else-if="estado === 'educacaoCrianca'" class="educacao-card">
            <div class="mensagem bot">
              <h3 class="educacao-titulo">Bebês e crianças</h3>
              <ul class="educacao-lista">
                <li><strong>Ao nascer:</strong> BCG, Hepatite B</li>
                <li><strong>2 a 6 meses:</strong> Penta, VIP, Pneumo 10, Rotavírus, Meningo C</li>
                <li><strong>6 meses:</strong> Influenza, COVID-19</li>
                <li><strong>12 a 15 meses:</strong> Tríplice viral, Tetraviral, Hepatite A, DTP, Polio, Febre Amarela
                </li>
              </ul>
              <button class="btn-educacao" @click="abrirCalendario">Saiba mais no Calendário Nacional</button>
            </div>
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>

          <div v-else-if="estado === 'educacaoAdolescente'" class="educacao-card">
            <div class="mensagem bot">
              <h3 class="educacao-titulo">Adolescentes e jovens (10 a 24 anos)</h3>
              <ul class="educacao-lista">
                <li>Hepatite B (3 doses, se não vacinado)</li>
                <li>dT (reforço a cada 10 anos)</li>
                <li>Tríplice viral (2 doses)</li>
                <li>HPV (1 dose de 9 a 14 anos)</li>
                <li>Meningocócica ACWY</li>
                <li>Febre Amarela e Varicela (casos específicos)</li>
              </ul>
              <button class="btn-educacao" @click="abrirCalendario">Saiba mais no Calendário Nacional</button>
            </div>
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>

          <div v-else-if="estado === 'educacaoAdulto'" class="educacao-card">
            <div class="mensagem bot">
              <h3 class="educacao-titulo">Adultos (25 a 59 anos)</h3>
              <ul class="educacao-lista">
                <li>Hepatite B (3 doses)</li>
                <li>dT (reforço a cada 10 anos)</li>
                <li>Tríplice viral (1 ou 2 doses, se não vacinado)</li>
                <li>Febre Amarela (dose única)</li>
                <li>Pneumocócica 23v e Varicela (grupos específicos)</li>
              </ul>
              <button class="btn-educacao" @click="abrirCalendario">Saiba mais no Calendário Nacional</button>
            </div>
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>

          <div v-else-if="estado === 'educacaoIdoso'" class="educacao-card">
            <div class="mensagem bot">
              <h3 class="educacao-titulo">Idosos (60+)</h3>
              <ul class="educacao-lista">
                <li>Hepatite B (3 doses)</li>
                <li>dT (reforço a cada 10 anos)</li>
                <li>Influenza (anual)</li>
                <li>COVID-19 (semestral)</li>
                <li>Pneumocócica 23v e Varicela</li>
                <li>Febre Amarela (em áreas de risco)</li>
              </ul>
              <button class="btn-educacao" @click="abrirCalendario">Saiba mais no Calendário Nacional</button>
            </div>
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>

          <div v-else-if="estado === 'educacaoGestante'" class="educacao-card">
            <div class="mensagem bot">
              <h3 class="educacao-titulo">Gestantes</h3>
              <ul class="educacao-lista">
                <li>dTpa (única a partir da 20ª semana)</li>
                <li>Influenza (anual)</li>
                <li>Hepatite B (3 doses, se necessário)</li>
                <li>COVID-19 (1 dose por gestação)</li>
              </ul>
              <button class="btn-educacao" @click="abrirCalendario">Saiba mais no Calendário Nacional</button>
            </div>
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>
          <div v-else-if="estado === 'suporte'">
            <div v-if="!subEstadoSuporte">
              <div class="mensagem bot">Como podemos te ajudar?</div>
              <div v-for="(item, index) in submenuSuporte" :key="index" class="opcao">
                <button @click="executarAcaoSuporte(item.acao)">{{ item.label }}</button>
              </div>
              <button class="voltar" @click="estado = 'inicio'">Voltar</button>
            </div>
            <div v-else-if="subEstadoSuporte === 'email'" class="suporte-box">
              <div class="mensagem bot bolha-titulo">
                {{ tipoMensagem === 'erro' ? 'Descreva o erro' : 'Descreva a melhoria' }}
              </div>
              <form @submit.prevent="enviarEmailSuporte" class="form-suporte">
                <label class="label" for="nome">Seu nome:</label>
                <input id="nome" v-model="formEmail.nome" required placeholder="Seu nome completo" />
                <label class="label" for="email">Email*:</label>
                <input id="email" type="email" v-model="formEmail.email" required placeholder="voce@exemplo.com" />
                <label class="label" for="mensagem">Mensagem*:</label>
                <textarea id="mensagem" v-model="formEmail.mensagem" rows="5" required
                  placeholder="O que aconteceu / sua sugestão em detalhes"></textarea>
                <small class="dica">
                  Enviaremos estas informações junto com dados técnicos seguros para agilizar o suporte.
                </small>
                <div class="botoes">
                  <button class="btn-acao" type="submit" :disabled="carregando">
                    {{ carregando ? 'Enviando...' : 'Enviar' }}
                  </button>
                  <button class="voltar" type="button" @click="subEstadoSuporte = null">Voltar</button>
                </div>
                <div v-if="sucesso" class="mensagem bot">{{ sucesso }}</div>
                <div v-if="erroEnvio" class="mensagem bot">{{ erroEnvio }}</div>
              </form>
            </div>
            <div v-else-if="subEstadoSuporte === 'ajuda'">
              <div class="mensagem bot">
                <p><strong>Informações de contato:</strong></p>
                <p>SAC: 0800 123 4567</p>
                <p>E-mail: suporte@doseemdia.com.br</p>
                <p>Atendimento: Seg a Sex, 08h às 18h</p>
              </div>
              <button class="voltar" @click="subEstadoSuporte = null">Voltar</button>
            </div>
          </div>
          <div v-else-if="estado === 'resposta' || estado === 'menu-voltar'">
            <div v-for="(msg, index) in mensagens" :key="index" class="mensagem bot" v-html="msg"></div>
            <button v-if="estado === 'menu-voltar'" class="voltar" @click="voltarAoMenu">
              Voltar ao menu
            </button>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

const baseURL = process.env.VUE_APP_API_BASE_URL || "https://doseemdiabackend-production.up.railway.app";

export const api = axios.create({
  baseURL: baseURL.replace(/\/+$/, ""),
  timeout: 20000,
});

export default {
  name: 'ChatBot',
  data() {
    return {
      visivel: false,
      estado: 'boasVindas',
      mensagens: [],
      nomeUsuario: localStorage.getItem('usuarioNome') || 'Usuário',
      subEstadoSuporte: null,
      tipoMensagem: null,
      carregando: false,
      sucesso: null,
      erroEnvio: null,
      formEmail: {
        nome: localStorage.getItem('usuarioNome') || '',
        email: localStorage.getItem('usuarioEmail') || '',
        assunto: '',
        mensagem: ''
      },
      opcoes: [
        { label: 'Informações sobre minhas vacinas', acao: 'irParaVacinas' },
        { label: 'Conta do usuário', acao: 'abrirConta' },
        { label: 'Educação em saúde', acao: 'abrirEducacao' },
        { label: 'Suporte', acao: 'abrirSuporte' }
      ],
      submenuVacinas: [
        { label: 'Minha caderneta digital', acao: 'irParaHome' },
        { label: 'Quais vacinas estão atrasadas?', acao: 'listarAtrasadas' },
        { label: 'Quais vacinas estão para atrasar?', acao: 'listarAVencer' },
        { label: 'Quais vacinas estão em dia?', acao: 'listarEmDia' }
      ],
      submenuConta: [
        { label: 'Alterar senha', acao: 'redefinirSenha' },
        { label: 'Editar dados pessoais', acao: 'editarPerfil' },
        { label: 'Excluir minha conta', acao: 'excluirConta' }
      ],
      submenuSuporte: [
        { label: 'Reportar erro no sistema', acao: 'reportarErro' },
        { label: 'Quero sugerir uma melhoria', acao: 'sugerirMelhoria' },
        { label: 'Outras dúvidas', acao: 'ajuda' }
      ]
    };
  },
  methods: {
    alternarChat() { this.visivel = !this.visivel; },
    fecharChat() {
      this.visivel = false;
      this.estado = 'boasVindas';
      this.mensagens = [];
      this.subEstadoSuporte = null;
    },
    voltarAoMenu() {
      this.estado = 'inicio';
      this.mensagens = [];
    },
    executarAcao(opcao) {
      switch (opcao.acao) {
        case 'irParaVacinas':
          this.mensagens = [];
          this.estado = 'vacinas';
          break;
        case 'abrirConta':
          this.mensagens = [];
          this.estado = 'conta';
          break;
        case 'abrirEducacao':
          this.mensagens = [];
          this.estado = 'educacao';
          break;
        case 'abrirSuporte':
          this.mensagens = [];
          this.estado = 'suporte';
          break;
        default:
          break;
      }
    },
    executarAcaoDireta(acao) {
      const rotas = {
        irParaHome: () => this.$router.push('/home'),
        listarAtrasadas: () => this.buscarVacinasPorStatus(2, 'Você está com as seguintes vacinas atrasadas:'),
        listarAVencer: () => this.buscarVacinasPorStatus(1, 'Essas são as suas vacinas que estão prestes a vencer:'),
        listarEmDia: () => this.buscarVacinasPorStatus(0, 'Parabéns por focar na sua saúde! Vacinas em dia:'),
        redefinirSenha: () => this.$router.push('/redefinir-senha'),
        editarPerfil: () => this.$router.push('/editar-perfil'),
        excluirConta: () => this.$router.push('/configuracoes')
      };
      rotas[acao] && rotas[acao]();
    },
    mostrarMensagem(msg) { this.mensagens.push(msg); },
    obterCpfOuAvisar() {
      const cpf = localStorage.getItem('usuarioCpf');
      if (!cpf) {
        this.mostrarMensagem('! Não foi possível identificar seu CPF. Faça login novamente.');
        this.estado = 'menu-voltar';
        return null;
      }
      return cpf;
    },
    async buscarVacinasPorStatus(statusDesejado, titulo) {
      this.mensagens = [];
      this.estado = 'resposta';

      const cpf = this.obterCpfOuAvisar();
      if (!cpf) return;

      this.mostrarMensagem('Certo! Estou buscando suas vacinas...');

      try {
        const { data: vacinas } = await api.get(`/api/vacinas/listaVacinas/${cpf}`);
        const filtradas = (vacinas || []).filter(v => v.status === statusDesejado);

        if (filtradas.length) {
          const lista = filtradas.map(v => `<li>${v.nome}</li>`).join('');
          this.mostrarMensagem(`${titulo}<ul>${lista}</ul>`);
        } else {
          const mensagensFallback = {
            2: 'Nenhuma vacina atrasada. Ótimo trabalho!',
            1: 'Nenhuma vacina está perto da data de vencimento!',
            0: 'Nenhuma vacina marcada como em dia no momento.'
          };
          this.mostrarMensagem(mensagensFallback[statusDesejado] || 'Nada encontrado.');
        }
      } catch (erro) {
        console.error(erro);
        this.mostrarMensagem('Ocorreu um erro ao buscar suas vacinas. Tente novamente mais tarde.');
      }

      this.estado = 'menu-voltar';
    },
    executarAcaoSuporte(acao) {
      this.sucesso = null; this.erroEnvio = null;
      if (acao === 'reportarErro') {
        this.tipoMensagem = 'erro';
        this.formEmail.assunto = '[Erro Reportado]';
        this.subEstadoSuporte = 'email';
      } else if (acao === 'sugerirMelhoria') {
        this.tipoMensagem = 'melhoria';
        this.formEmail.assunto = '[Sugestão de Melhoria]';
        this.subEstadoSuporte = 'email';
      } else if (acao === 'ajuda') {
        this.subEstadoSuporte = 'ajuda';
      }
    },
    async enviarEmailSuporte() {
      this.carregando = true; this.sucesso = null; this.erroEnvio = null;
      try {
        const payload = {
          tipo: this.tipoMensagem,
          nome: this.formEmail.nome,
          email: this.formEmail.email,
          assunto: this.formEmail.assunto,
          mensagem: this.formEmail.mensagem
        };
        await api.post(`/api/suporte/mensagem`, payload);
        this.sucesso = 'Mensagem enviada com sucesso. Em breve entraremos em contato.';
        this.formEmail.mensagem = '';
      } catch (e) {
        console.error(e);
        this.erroEnvio = 'Não foi possível enviar agora. Tente novamente ou use o e-mail suporte@doseemdia.com.br.';
      } finally {
        this.carregando = false;
      }
    },
    abrirCalendario() {
      window.open('https://www.gov.br/saude/pt-br/vacinacao/calendario', '_blank');
    }
  }
};
</script>

<style scoped>
.chatbot-toggle {
  position: fixed;
  bottom: 20px;
  right: 20px;
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  z-index: 1000;
}

.chatbot-toggle img {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  transition: transform 0.2s;
}

.chatbot-toggle img:hover {
  transform: scale(1.05);
}

.chatbot-popup {
  position: fixed;
  bottom: 90px;
  right: 20px;
  z-index: 999;
  width: 410px;
  height: 555px;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  padding: 30px 20px 20px 20px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

@media (max-width: 480px) {
  .chatbot-popup {
    width: 95vw;
    height: 90vh;
  }
}

.fechar-chat {
  position: absolute;
  top: 12px;
  right: 12px;
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px;
  transition: transform 0.2s ease;
}

.fechar-chat img {
  width: 20px;
  height: 20px;
  opacity: 0.7;
}

.fechar-chat:hover img {
  transform: rotate(90deg);
  opacity: 1;
}


.mensagem {
  margin-bottom: 15px;
}

.opcao button,
.iniciar {
  display: block;
  margin: 5px 0;
  width: 100%;
  padding: 10px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

.opcao button {
  background: #ffb894;
}

.opcao button:hover,
.voltar:hover {
  background: #e0e0e0;
}

.voltar {
  position: absolute;
  bottom: 15px;
  left: 20px;
  right: 15px;
  background-color: #ffb894;
  border: none;
  padding: 12px;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
}

.iniciar {
  background: #007bff;
  color: white;
  font-weight: bold;
}

.iniciar:hover {
  background: #0056b3;
}

.resposta {
  color: #333;
  margin-top: 20px;
}

.chat-fade-enter-active,
.chat-fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.chat-fade-enter-from,
.chat-fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

.fechar-chat img {
  width: 20px;
  height: 20px;
}

.mensagem.bot {
  background: #ffe5cc;
  padding: 12px 16px;
  border-radius: 18px 18px 18px 4px;
  margin-bottom: 12px;
  max-width: 90%;
  word-wrap: break-word;
  font-size: 0.95rem;
  color: #1f1f1f;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.06);
  font-family: 'Segoe UI', 'Roboto', sans-serif;
}

.opcoes-chat {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 10px;
}

.btn-opcao {
  background: #f0f0f0;
  border: none;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  text-align: left;
}

.opcao button {
  display: block;
  width: 100%;
  padding: 12px 16px;
  background-color: #f1d7ff;
  border: none;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
  cursor: pointer;
  text-align: left;
  transition: all 0.2s ease;
  font-family: 'Segoe UI', 'Roboto', sans-serif;
}

.opcao button:hover,
.voltar:hover {
  background-color: #e0e0e0;
}

.voltar {
  position: absolute;
  bottom: 15px;
  left: 20px;
  right: 20px;
  background-color: #f0f0f0;
  border: none;
  padding: 12px;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  text-align: center;
}

.titulo-vacina {
  font-size: 1.1rem;
  font-weight: 600;
  color: #de2e03;
  margin-bottom: 10px;
}

.voltarEducacao {
  margin-top: -80px !important;
}

.suporte-box {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.form-suporte {
  display: flex !important;
  flex-direction: column;
  align-items: center;
  width: 100%;
  max-width: 360px;
  margin: 0 auto;
  gap: 12px;
  padding: 8px 6px;
  box-sizing: border-box;
  grid-template-columns: unset !important
}

.bolha-titulo {
  margin: 0 auto 12px auto;
  border-radius: 18px 18px 18px 4px;
  max-width: 380px;
  width: 100%;
  margin-left: 1px;
}

.form-suporte label {
  width: 100%;
  font-weight: 700;
  color: #1f1f1f;
  text-align: center !important;
}

.form-suporte label[for="mensagem"],
.form-suporte label[for="email"],
.form-suporte label[for="nome"] {
  text-align: left !important;
}

.form-suporte input,
.form-suporte textarea {
  width: 100%;
  max-width: 320px;
  padding: 10px 12px;
  border: 2px solid #1f1f1f;
  border-radius: 6px;
  background: #fff;
  outline: none;
  font-size: 0.95rem;
  text-align: center;
}

.dica {
  display: block;
  margin-top: 4px;
  color: #6b6b6b;
  font-size: 0.85rem;
}

.botoes {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 8px;
}

.btn-acao {
  position: absolute;
  left: 20px;
  right: 20px;
  background-color: #f0f0f0;
  border: none;
  padding: 12px;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  text-align: center;
  margin-top: -10px;
}

.btn-acao:hover {
  background-color: #e0e0e0;
}

@media (max-width: 480px) {
  .form-suporte {
    max-width: 100%;
    grid-template-columns: 1fr;
  }

  .form-suporte label {
    justify-self: start;
  }

  .grid-span-2 {
    grid-column: 1 / 2;
  }
}

.grid-educacao {
  display: grid;
  grid-template-columns: 1fr;
  gap: 10px;
  margin-top: 8px;
}

.card-atalho {
  width: 100%;
  padding: 12px 16px;
  background-color: #f1d7ff;
  border: none;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  color: #333;
  cursor: pointer;
  text-align: left;
  transition: all 0.2s ease;
  font-family: 'Segoe UI', 'Roboto', sans-serif;
}

.card-atalho:hover {
  background-color: #e6c6ff;
}

.educacao-card .mensagem.bot {
  background: #ffe5cc;
}

.educacao-titulo {
  font-size: 1.05rem;
  font-weight: 700;
  color: black;
  margin-bottom: 8px;
}

.educacao-lista {
  margin: 8px 0 12px 14px;
  padding: 0;
}

.educacao-lista li {
  margin: 5px 0;
}

.educacao-lista li::marker {
  color: black;
}

.btn-educacao {
  display: inline-block;
  padding: 10px 14px;
  background: #f6f6f6;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: background .2s ease;
}

.btn-educacao:hover {
  background: #e0e0e0;
}

@media (min-width: 420px) {
  .grid-educacao {
    grid-template-columns: 1fr 1fr;
  }
}

.chatbot-popup {
  padding-bottom: 80px;
}
</style>