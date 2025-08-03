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
              <div class="saudacao">Olá, {{ nomeUsuario }}!</div>
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
          <div v-else-if="estado === 'educacaoCrianca'">
            <div class="mensagem bot">
              <h3 class="titulo-vacina">Bebês e crianças</h3>
              <ul>
                <li><strong>Ao nascer:</strong> BCG, Hepatite B</li>
                <li><strong>2 a 6 meses:</strong> Penta, VIP, Pneumo 10, Rotavírus, Meningo C</li>
                <li><strong>6 meses:</strong> Influenza, COVID-19</li>
                <li><strong>12 a 15 meses:</strong> Tríplice viral, Tetraviral, Hepatite A, DTP, Polio, Febre Amarela
                </li>
              </ul>
              <a href="https://www.gov.br/saude/pt-br/vacinacao/calendario" target="_blank" class="btn-saiba-mais">
                Clique aqui para acessar mais informações sobre as vacinações!
              </a>
            </div>
          </div>
          <div v-if="estado === 'educacaoCrianca'">
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
          <div v-else-if="estado === 'educacaoAdulto'" class="mensagem bot">
            <p><strong>Adultos (25 a 59 anos):</strong><br />
              - Hepatite B (3 doses)<br />
              - dT (reforço a cada 10 anos)<br />
              - SCR (1 ou 2 doses, se não vacinado)<br />
              - Febre Amarela (dose única)<br />
              - Pneumocócica 23v e Varicela (grupos específicos)</p>
            <br /><a href="https://www.gov.br/saude/pt-br/vacinacao/calendario">Veja detalhes na página</a>.
            <button class="voltarEducacao" @click="estado = 'educacao'">Voltar</button>
          </div>
          <div v-else-if="estado === 'educacaoIdoso'" class="mensagem bot">
            <p><strong>Idosos (60+):</strong><br />
              - Hepatite B (3 doses)<br />
              - dT (reforço a cada 10 anos)<br />
              - Influenza (anual)<br />
              - COVID-19 (semestral)<br />
              - Pneumocócica 23v e Varicela<br />
              - Febre Amarela (para residentes ou viajantes a áreas de risco)<br /></p>
            <br /><a href="https://www.gov.br/saude/pt-br/vacinacao/calendario">Veja detalhes na página</a>.
            <button class="voltar" @click="estado = 'educacao'">Voltar</button>
          </div>
          <div v-else-if="estado === 'educacaoGestante'" class="mensagem bot">
            <p><strong>Gestantes:</strong><br />
              - dTpa (única a partir da 20ª semana)<br />
              - Influenza (anual)<br />
              - Hepatite B (3 doses, se necessário)<br />
              - COVID-19 (1 dose por gestação)
              <br />Vacinas protegem a gestante e o bebê. Consulte sua equipe de pré-natal!
            </p>
            <br /><a href="https://www.gov.br/saude/pt-br/vacinacao/calendario" target="_blank" class="link-educacao">
              Clique aqui para acessar mais informações sobre as vacinações.
            </a>
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
      nomeUsuario: localStorage.getItem("usuarioNome") || "Usuário",
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
    },
    async selecionarOpcao(opcao) {
      this.mensagens.push({ texto: opcao.label, tipo: 'usuario' });
      await this.executarAcao(opcao);
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
  background: #e8f0fe;
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

.opcao button,
.voltar {
  display: block;
  width: 100%;
  padding: 12px 16px;
  background-color: #f6f6f6;
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

.titulo-vacina {
  font-size: 1.1rem;
  font-weight: 600;
  color: #de2e03;
  margin-bottom: 10px;
}

.voltarEducacao {
  margin-top: -80px !important;
}

</style>