<template>
  <div>
    <!-- Botão flutuante com imagem -->
    <button class="chatbot-toggle" @click="alternarChat" ref="botao">
      <img src="@/imagens/ChatBotVitta.png" alt="Abrir chatbot" />
    </button>

    <!-- Transição suave apenas ao abrir/fechar com o botão -->
    <div v-show="visivel" ref="chat" class="chatbot-popup chatbot">
      <transition name="chat-fade">
        <div>
          <button class="fechar-chat" @click="fecharChat">
            <img src="@/assets/icons/Icone-fechar-chatbot.svg" alt="Fechar chatbot" />
          </button>

          <!-- Saudacao + Menu principal -->
          <div v-if="estado === 'boasVindas' || estado === 'inicio'">
            <div class="mensagem bot">
              <div class="saudacao">Olá! Bem-vindo ao Dose em Dia!</div>
              <p>Eu sou a Vitta, sua assistente virtual. Estou aqui para te ajudar com vacinas, informações de saúde,
                sua conta e suporte técnico. Será um prazer te auxiliar!</p>
              <p>Como posso te ajudar hoje?</p>
            </div>
            <div v-for="(opcao, index) in opcoes" :key="index" class="opcao">
              <button @click="executarAcao(opcao)">{{ opcao.label }}</button>
            </div>
          </div>

          <!-- Submenus -->
          <div v-else-if="estado === 'vacinas'">
            <div class="mensagem bot">O que você quer saber mais sobre suas vacinas?</div>
            <div v-for="(item, index) in submenuVacinas" :key="index" class="opcao">
              <button @click="item.acao ? executarAcaoDireta(item.acao) : mostrarResposta(item.resposta)">{{ item.label
              }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <div v-else-if="estado === 'conta'">
            <div class="mensagem bot">O que deseja fazer em sua conta?</div>
            <div v-for="(item, index) in submenuConta" :key="index" class="opcao">
              <button @click="item.acao ? executarAcaoDireta(item.acao) : mostrarResposta(item.resposta)">{{ item.label
              }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <div v-else-if="estado === 'educacao'">
            <div class="mensagem bot">Sobre qual tema de saúde você quer saber mais?</div>
            <div v-for="(item, index) in submenuEducacao" :key="index" class="opcao">
              <button @click="executarAcao(item)">{{ item.label }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>
          <div v-else-if="estado === 'educacaoCrianca'" class="mensagem bot">
            <p><strong>Bebês e crianças:</strong><br />
              - Ao nascer: BCG e Hepatite B<br />
              - 2 a 6 meses: Penta, VIP, Pneumocócica 10, Rotavírus, Meningocócica C<br />
              - 6 meses: Influenza e COVID-19<br />
              - 12 a 15 meses: Tríplice viral, Tetraviral, Hepatite A, DTP, Poliomielite, Febre Amarela</p>
            <br /><a href="https://www.gov.br/saude/pt-br/vacinacao/calendario">Veja detalhes na página</a>.
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>
          <div v-else-if="estado === 'educacaoAdolescente'" class="mensagem bot">
            <p><strong>Adolescentes e jovens (10 a 24 anos):</strong><br />
              - Hepatite B (3 doses, se não vacinado)<br />
              - dT (reforço a cada 10 anos)<br />
              - SCR (tríplice viral, 2 doses)<br />
              - HPV (1 dose de 9 a 14 anos)<br />
              - Meningocócica ACWY<br />
              - Febre Amarela e Varicela (casos específicos)</p>
              <br /><a href="https://www.gov.br/saude/pt-br/vacinacao/calendario">Veja detalhes na página</a>.
              <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>
          <div v-else-if="estado === 'suporte'">
            <div class="mensagem bot">Como podemos te ajudar?</div>
            <div v-for="(item, index) in submenuSuporte" :key="index" class="opcao">
              <button @click="mostrarResposta(item.resposta)">{{ item.label }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <!-- Resposta final -->
          <div v-for="(msg, index) in mensagens" :key="index" class="mensagem bot" v-html="msg"></div>

          <div v-if="estado === 'menu-voltar'">
            <button class="voltar" @click="estado = 'inicio'; mensagens = []">Voltar ao menu</button>
          </div>

        </div>
      </transition>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: "ChatBot",
  data() {
    return {
      visivel: false,
      estado: "boasVindas",
      mensagens: [],
      resposta: null,
      opcoes: [
        { label: "Informações sobre minhas vacinas", acao: "irParaVacinas" },
        { label: "Conta do usuário", acao: "abrirConta" },
        { label: "Educação em saúde", acao: "abrirEducacao" },
        { label: "Suporte", acao: "abrirSuporte" }
      ],
      submenuVacinas: [
        { label: "Minha cardeneta digital", acao: "irParaHome" },
        { label: "Quais vacinas estão atrasadas?", acao: "listarAtrasadas" },
        { label: "Quais vacinas estão para atrasar?", acao: "listarAVencer" },
        { label: "Quais vacinas estão em dia?", acao: "listarEmDia" }
      ],
      submenuConta: [
        { label: "Alterar senha", acao: "redefinirSenha" },
        { label: "Editar dados pessoais", acao: "editarPerfil" },
        { label: "Excluir minha conta", resposta: "Envie uma solicitação para <strong>privacidade@doseemdia.com.br</strong>." }
      ],
      submenuEducacao: [
        { label: "Vacinas para bebês e crianças", acao: "educacaoCrianca" },
        { label: "Vacinas para adolescentes e jovens", acao: "educacaoAdolescente" },
        { label: "Vacinas para adultos", acao: "educacaoAdulto" },
        { label: "Vacinas para idosos", acao: "educacaoIdoso" },
        { label: "Vacinas para gestantes", acao: "educacaoGestante" }
      ],
      submenuSuporte: [
        { label: "Esqueci minha senha", resposta: "Use a opção 'Recuperar senha' na tela de login." },
        { label: "Reportar erro no sistema", resposta: "Envie detalhes para <strong>suporte@doseemdia.com.br</strong>." },
        { label: "Quero sugerir uma melhoria", resposta: "<a href=\"mailto:contato@doseemdia.com.br?subject=Sugestão%20para%20o%20Dose%20em%20Dia&body=Olá%20equipe,%20gostaria%20de%20sugerir...\"><strong>contato@doseemdia.com.br</strong></a>" },
        { label: "Outras dúvidas", resposta: "Consulte nossa <a href='/faq'>FAQ</a> ou entre em contato." }
      ]
    };
  },
  methods: {
    alternarChat() {
      this.visivel = !this.visivel;
    },
    fecharChat() {
      this.visivel = false;
      this.estado = "boasVindas";
      this.resposta = null;
      this.mensagens = [];
    },
    executarAcao(opcao) {
      this.resposta = null;
      switch (opcao.acao) {
        case "irParaVacinas":
          this.mensagens = [];
          this.estado = "vacinas";
          break;
        case "abrirConta":
          this.mensagens = [];
          this.estado = "conta";
          break;
        case "abrirEducacao":
          this.mensagens = [];
          this.estado = "educacao";
          break;
        case "abrirSuporte":
          this.mensagens = [];
          this.estado = "suporte";
          break;
        case "listarAtrasadas":
          this.mensagens = [];
          this.buscarVacinasAtrasadas();
          break;
        case "educacaoCrianca":
          this.estado = "educacaoCrianca";
          break;
        case "educacaoAdolescente":
          this.estado = "educacaoAdolescente";
          break;
        case "educacaoAdulto":
          this.estado = "educacaoAdulto";
          break;
        case "educacaoIdoso":
          this.estado = "educacaoIdoso";
          break;
        case "educacaoGestante":
          this.estado = "educacaoGestante";
          break;
        default:
          this.mostrarResposta(opcao.resposta);
      }
    },
    executarAcaoDireta(acao) {
      if (acao === "irParaHome") {
        this.$router.push("/home");
      } else if (acao === "listarAtrasadas") {
        this.buscarVacinasAtrasadas();
      } else if (acao === "listarAVencer") {
        this.buscarVacinasAVencer();
      } else if (acao === "listarEmDia") {
        this.buscarVacinasEmDia();
      } else if (acao === "redefinirSenha") {
        this.$router.push("/redefinir-senha");
      } else if (acao === "editarPerfil") {
        this.$router.push("/editar-perfil");
      }
    },
    mostrarResposta(msg) {
      this.resposta = msg;
      this.estado = "inicio";
    },
    mostrarMensagem(msg) {
      this.mensagens.push(msg);
    },
    async buscarVacinasAtrasadas() {
      this.mensagens = [];
      this.estado = "resposta";

      const cpf = localStorage.getItem("usuarioCPF");

      if (!cpf) {
        this.mostrarMensagem("! Não foi possível identificar seu CPF. Faça login novamente.");
        return;
      }

      this.mostrarMensagem("Certo! Estou buscando suas vacinas...");

      try {
        const response = await axios.get(`http://localhost:5054/api/vacinas/listaVacinas/${cpf}`);
        const vacinas = response.data;

        const atrasadas = vacinas.filter(v => v.status === 2); // 2 = Vencida

        if (atrasadas.length) {
          const lista = atrasadas.map(v => `<li>${v.nome}</li>`).join("");
          this.mostrarMensagem(`Você está com as seguintes vacinas atrasadas:<ul>${lista}</ul>`);
        } else {
          this.mostrarMensagem("Todas as suas vacinas estão em dia. Parabéns!");
        }
      } catch (erro) {
        console.error(erro);
        this.mostrarMensagem("Ocorreu um erro ao buscar suas vacinas. Tente novamente mais tarde.");
      }

      this.estado = "menu-voltar";
    },
    async buscarVacinasAVencer() {
      this.mensagens = [];
      this.estado = "resposta";

      const cpf = localStorage.getItem("usuarioCPF");

      if (!cpf) {
        this.mostrarMensagem("! Não foi possível identificar seu CPF. Faça login novamente.");
        return;
      }

      this.mostrarMensagem("Certo! Estou buscando suas vacinas...");

      try {
        const response = await axios.get(`http://localhost:5054/api/vacinas/listaVacinas/${cpf}`);
        const vacinas = response.data;

        const atrasadas = vacinas.filter(v => v.status === 1); // 1 = A Vencer

        if (atrasadas.length) {
          const lista = atrasadas.map(v => `<li>${v.nome}</li>`).join("");
          this.mostrarMensagem(`Essas são as suas vacinas que estão prestes a vencer:<ul>${lista}</ul>`);
        } else {
          this.mostrarMensagem("Todas as suas vacinas estão em dia. Parabéns!");
        }
      } catch (erro) {
        console.error(erro);
        this.mostrarMensagem("Ocorreu um erro ao buscar suas vacinas. Tente novamente mais tarde.");
      }

      this.estado = "menu-voltar";
    },
    async buscarVacinasEmDia() {
      this.mensagens = [];
      this.estado = "resposta";

      const cpf = localStorage.getItem("usuarioCPF");

      if (!cpf) {
        this.mostrarMensagem("! Não foi possível identificar seu CPF. Faça login novamente.");
        return;
      }

      this.mostrarMensagem("Certo! Estou buscando suas vacinas...");

      try {
        const response = await axios.get(`http://localhost:5054/api/vacinas/listaVacinas/${cpf}`);
        const vacinas = response.data;

        const atrasadas = vacinas.filter(v => v.status === 0); // 0 = Aplicadas em dia

        if (atrasadas.length) {
          const lista = atrasadas.map(v => `<li>${v.nome}</li>`).join("");
          this.mostrarMensagem(`Parabéns por focar na sua sáude, olha quantas vacinas em dia!:<ul>${lista}</ul>`);
        } else {
          this.mostrarMensagem("Todas as suas vacinas estão em dia. Parabéns!");
        }
      } catch (erro) {
        console.error(erro);
        this.mostrarMensagem("Ocorreu um erro ao buscar suas vacinas. Tente novamente mais tarde.");
      }

      this.estado = "menu-voltar";
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
  max-width: 450px;
  width: 95vw;
  background: white;
  border-radius: 10px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  padding: 25px;
}

.fechar-chat {
  position: absolute;
  top: 10px;
  right: 10px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
}

.mensagem {
  margin-bottom: 15px;
}

.opcao button,
.voltar,
.iniciar {
  display: block;
  margin: 5px 0;
  width: 100%;
  padding: 10px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

.opcao button,
.voltar {
  background: #f0f0f0;
}

.voltar {
  margin-top: 25px;
}

.opcao button:hover,
.voltar:hover {
  background: #e0e0e0;
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
  background: #f1f1f1;
  padding: 10px 15px;
  border-radius: 15px;
  margin-bottom: 10px;
  max-width: 90%;
  word-wrap: break-word;
  font-size: 0.95rem;
  color: #333;
  box-shadow: 1px 1px 4px rgba(0, 0, 0, 0.08);
}
</style>