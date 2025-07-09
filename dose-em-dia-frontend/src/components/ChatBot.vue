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
              <button @click="mostrarResposta(item.resposta)">{{ item.label }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <div v-else-if="estado === 'conta'">
            <div class="mensagem bot">O que deseja fazer em sua conta?</div>
            <div v-for="(item, index) in submenuConta" :key="index" class="opcao">
              <button @click="mostrarResposta(item.resposta)">{{ item.label }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <div v-else-if="estado === 'educacao'">
            <div class="mensagem bot">Sobre qual tema de saúde você quer saber mais?</div>
            <div v-for="(item, index) in submenuEducacao" :key="index" class="opcao">
              <button @click="mostrarResposta(item.resposta)">{{ item.label }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <div v-else-if="estado === 'suporte'">
            <div class="mensagem bot">Como podemos te ajudar?</div>
            <div v-for="(item, index) in submenuSuporte" :key="index" class="opcao">
              <button @click="mostrarResposta(item.resposta)">{{ item.label }}</button>
            </div>
            <button class="voltar" @click="estado = 'inicio'">Voltar</button>
          </div>

          <!-- Resposta final -->
          <div v-if="resposta" class="mensagem resposta" v-html="resposta"></div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script>
export default {
  name: "ChatBot",
  data() {
    return {
      visivel: false,
      estado: "boasVindas",
      resposta: null,
      opcoes: [
        { label: "Informações sobre minhas vacinas", acao: "irParaVacinas" },
        { label: "Conta do usuário", acao: "abrirConta" },
        { label: "Educação em saúde", acao: "abrirEducacao" },
        { label: "Suporte", acao: "abrirSuporte" }
      ],
      submenuVacinas: [
        { label: "Minha cardeneta digital", resposta: "Veja o calendário completo em: /calendario-vacinas" },
        { label: "Quais vacinas estão atrasadas?", resposta: "Acesse nossa tabela interativa em: /doencas-e-vacinas" },
        { label: "Quais vacinas estão para atrasar?", resposta: "Confira campanhas de vacinação em andamento na sua região." },
        { label: "Quais vacinas estão em dia?", resposta: "Veja respostas em: /faq-vacinas" }
      ],

      submenuConta: [
        { label: "Alterar senha", resposta: "Acesse <a href='/perfil'>Perfil</a> e clique em 'Alterar Senha'." },
        { label: "Editar dados pessoais", resposta: "Você pode editar seus dados no menu <a href='/perfil'>Perfil</a>." },
        { label: "Excluir minha conta", resposta: "Envie uma solicitação para <strong>privacidade@doseemdia.com.br</strong>." }
      ],
      submenuEducacao: [
        { label: "Vacinas para bebês e crianças", resposta: "Veja mais em <a href='/educacao-infantil'>nossa página de vacinas infantis</a>." },
        { label: "Vacinas para adultos", resposta: "Exemplos importantes: dT, Hepatite B, COVID-19." },
        { label: "Vacinas para idosos", resposta: "Gripe, Pneumo 23, Herpes-zóster." },
        { label: "Vacinas para gestantes", resposta: "dTpa e Influenza são essenciais durante a gestação." },
        { label: "Atualizações sobre COVID-19", resposta: "Consulte campanhas ativas e reforços disponíveis." }
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
    },
    executarAcao(opcao) {
      this.resposta = null;
      switch (opcao.acao) {
        case "irParaVacinas":
          this.estado = "vacinas";
          break;
        case "abrirConta":
          this.estado = "conta";
          break;
        case "abrirEducacao":
          this.estado = "educacao";
          break;
        case "abrirSuporte":
          this.estado = "suporte";
          break;
        default:
          this.mostrarResposta(opcao.resposta);
      }
    },
    mostrarResposta(msg) {
      this.resposta = msg;
      this.estado = "inicio";
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
</style>