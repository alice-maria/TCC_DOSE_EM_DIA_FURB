<template>
    <v-container fluid class="pa-0">
        <div class="pagina-poliprivacidade">
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
                        <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt="" class="breadcrumb-home-img" />
                        {{ item.text }}
                    </span>
                </template>
            </v-breadcrumbs>
            <div class="conteudo-politica">
                <div class="logo-container">
                    <img id="topo" src="@/imagens/logo.png" alt="Logo" class="logo" @click="$router.push('/home')" />
                </div>

                <h4 class="titulo-politica">POLÍTICA DE PRIVACIDADE DO DOSE EM DIA</h4>

                <h1 class="introducao-politica">
                    Quando você usa nossos serviços, está confiando a nós suas informações.
                    Entendemos que isso é uma grande responsabilidade e trabalhamos duro para
                    proteger essas informações e colocar você no controle.
                </h1>

                <p class="paragrafo-politica">
                    Esta Política de Privacidade destina-se a ajudar você a entender quais informações
                    coletamos, por que as coletamos e como você pode atualizar, gerenciar, exportar e
                    excluir essas informações.
                </p>

                <p class="topicos_politica">
                    A sua privacidade é importante para nós. É política do Dose em Dia respeitar a sua privacidade em
                    relação a qualquer
                    informação sua que possamos coletar no site Dose em Dia e outros sites que possuímos e operamos.
                </p>

                <p class="topicos_politica">
                    Solicitamos informações pessoais apenas quando realmente precisamos delas para lhe fornecer um
                    serviço.
                    Fazemo-lo
                    por meios justos e legais, com o seu conhecimento e consentimento. Também informamos por que estamos
                    coletando e
                    como será usado.
                </p>
                <p class="topicos_politica">
                    Apenas retemos as informações coletadas pelo tempo necessário para fornecer o serviço solicitado.
                    Quando
                    armazenamos
                    dados, protegemos dentro de meios comercialmente aceitáveis para evitar perdas e roubos, bem como
                    acesso,
                    divulgação, cópia, uso ou modificação não autorizados. Não compartilhamos informações de
                    identificação
                    pessoal
                    publicamente ou com terceiros, exceto quando exigido por lei.
                </p>
                <p class="topicos_politica">
                    O nosso site pode ter links para sites externos que não são operados por nós. Esteja ciente de que
                    não
                    temos
                    controle sobre o conteúdo e práticas desses sites e não podemos aceitar responsabilidade por suas
                    respectivas
                    políticas de privacidade.
                </p>
                <p class="topicos_politica">
                    Você é livre para recusar a nossa solicitação de informações pessoais, entendendo que talvez não
                    possamos fornecer
                    alguns dos serviços desejados.
                </p>
                <p class="topicos_politica">
                    O uso continuado de nosso site será considerado como aceitação de nossas práticas em torno de
                    privacidade e
                    informações pessoais. Se você tiver alguma dúvida sobre como lidamos com dados do usuário e
                    informações
                    pessoais,
                    entre em contato conosco.
                </p>

                <h4 class="titulo_topicos_politica">Informações sobre vacinas
                </h4>

                <p class="topicos_politica">
                    As informações sobre vacinas apresentadas neste site são fictícias e utilizadas apenas para fins
                    ilustrativos e
                    educacionais. Elas não substituem orientação médica profissional. Para informações reais sobre
                    vacinação, consulte
                    órgãos de saúde competentes ou um profissional da área médica.
                </p>

                <h4 class="titulo_topicos_politica">Compromisso do Usuário
                </h4>

                <p class="topicos_politica">
                    O usuário se compromete a fazer uso adequado dos conteúdos e da informação que o Dose em Dia oferece
                    no
                    site e com
                    caráter enunciativo, mas não limitativo:
                <ul class="lista">
                    <li>A) Não se envolver em atividades que sejam ilegais ou contrárias à boa fé e à ordem pública;
                    </li>
                    <li>B) Não difundir propaganda ou conteúdo de natureza racista, xenofóbica, jogos de sorte ou azar,
                        qualquer
                        tipo de pornografia ilegal, de apologia ao terrorismo ou contra os direitos humanos;</li>
                    <li>C) Não causar danos aos sistemas físicos (hardwares) e lógicos (softwares) do Dose em Dia, de
                        seus
                        fornecedores ou terceiros, nem introduzir ou disseminar vírus informáticos ou quaisquer outros
                        sistemas de
                        hardware ou software que sejam capazes de causar danos anteriormente mencionados.</li>
                </ul>
                </p>

                <h4 class="titulo_topicos_politica">1. Termos
                </h4>

                <p class="topicos_politica">
                    Ao acessar o site Dose em Dia, você concorda em cumprir estes termos de serviço, todas as leis e
                    regulamentos
                    aplicáveis, e concorda que é responsável pelo cumprimento de todas as leis locais aplicáveis. Se
                    você não
                    concordar com algum desses termos, está proibido de usar ou acessar este site. Os materiais contidos
                    neste site
                    são protegidos pelas leis de direitos autorais e marcas comerciais aplicáveis.
                </p>
                <h4 class="titulo_topicos_politica">2. Uso de Licença
                </h4>

                <p class="topicos_politica">
                    É concedida permissão para baixar temporariamente uma cópia dos materiais (informações ou software)
                    no
                    site Dose em
                    Dia, apenas para visualização transitória pessoal e não comercial. Esta é a concessão de uma
                    licença,
                    não uma
                    transferência de título, e sob esta licença, você não pode:
                </p>
                <ul class="lista-politica">
                    <li>Modificar ou copiar os materiais;</li>
                    <li>Usar os materiais para qualquer finalidade comercial ou para exibição pública (comercial ou não
                        comercial);
                    </li>
                    <li>Tentar descompilar ou fazer engenharia reversa de qualquer software contido no site Dose em Dia;
                    </li>
                    <li>Remover quaisquer direitos autorais ou outras notações de propriedade dos materiais;</li>
                    <li>Transferir os materiais para outra pessoa ou 'espelhar' os materiais em qualquer outro servidor.
                    </li>
                </ul>
                <p class="topicos_politica">
                    Esta licença será automaticamente rescindida se você violar alguma dessas restrições e poderá ser
                    rescindida por
                    Dose em Dia a qualquer momento. Ao encerrar a visualização desses materiais ou após o término desta
                    licença, você
                    deve apagar todos os materiais baixados em sua posse, seja em formato eletrônico ou impresso.
                </p>
                <h4 class="titulo_topicos_politica">3. Isenção de responsabilidade
                </h4>

                <p class="topicos_politica">
                    Os materiais no site da Dose em Dia são fornecidos "como estão". Dose em Dia não oferece garantias,
                    expressas ou
                    implícitas, e, por este meio, isenta e nega todas as outras garantias, incluindo, sem limitação,
                    garantias
                    implícitas ou condições de comercialização, adequação a um fim específico ou não violação de
                    propriedade
                    intelectual
                    ou outra violação de direitos. Além disso, o Dose em Dia não garante ou faz qualquer representação
                    relativa à
                    precisão, aos resultados prováveis ou à confiabilidade do uso dos materiais em seu site ou de outra
                    forma
                    relacionado a esses materiais ou em sites vinculados a este site.
                </p>
                <h4 class="titulo_topicos_politica">4. Limitações
                </h4>
                <p class="topicos_politica">
                    Em nenhum caso o Dose em Dia ou seus fornecedores serão responsáveis por quaisquer danos (incluindo,
                    sem
                    limitação,
                    danos por perda de dados ou lucro ou devido a interrupção dos negócios) decorrentes do uso ou da
                    incapacidade de
                    usar os materiais em Dose em Dia, mesmo que Dose em Dia ou um representante autorizado da Dose em
                    Dia
                    tenha sido
                    notificado oralmente ou por escrito da possibilidade de tais danos. Como algumas jurisdições não
                    permitem limitações
                    em garantias implícitas ou limitações de responsabilidade por danos consequentes ou incidentais,
                    essas
                    limitações
                    podem não se aplicar a você.
                </p>
                <h4 class="titulo_topicos_politica">5. Precisão dos materiais
                </h4>
                <p class="topicos_politica">
                    Os materiais exibidos no site da Dose em Dia podem incluir erros técnicos, tipográficos ou
                    fotográficos.
                    Dose em Dia
                    não garante que qualquer material em seu site seja preciso, completo ou atual. Dose em Dia pode
                    fazer
                    alterações nos
                    materiais contidos em seu site a qualquer momento, sem aviso prévio. No entanto, Dose em Dia não se
                    compromete a
                    atualizar os materiais.
                </p>
                <h4 class="titulo_topicos_politica">6. Links
                </h4>
                <p class="topicos_politica">
                    O Dose em Dia não analisou todos os sites vinculados ao seu site e não é responsável pelo conteúdo
                    de
                    nenhum site
                    vinculado. A inclusão de qualquer link não implica endosso por Dose em Dia do site. O uso de
                    qualquer
                    site vinculado
                    é por conta e risco do usuário.
                </p>
                <h4 class="titulo_topicos_politica">Modificações
                </h4>
                <p class="topicos_politica">
                    O Dose em Dia pode revisar estes termos de serviço do site a qualquer momento, sem aviso prévio. Ao
                    usar
                    este site,
                    você concorda em ficar vinculado à versão atual desses termos de serviço.
                </p>
                <h4 class="titulo_topicos_politica">Lei aplicável
                </h4>

                <p class="topicos_politica">
                    Estes termos e condições são regidos e interpretados de acordo com as leis do Dose em Dia, e você se
                    submete
                    irrevogavelmente à jurisdição exclusiva dos tribunais naquele estado ou localidade.
                </p>
            </div>
        </div>

        <div class="voltar-topo" @click="voltarAoTopo">
            <img src="@/assets/icons/voltar-topo.svg" alt="Voltar ao topo" class="icone-voltar-topo" />
        </div>
    </v-container>
</template>

<script>

export default {
    data() {
        return {
            nomeUsuario: "",
            breadcrumbs: [
                { text: 'Serviços e Informações', to: '/home', icon: 'mdi-home' },
                { text: 'Configurações', to: '/configuracoes' },
                { text: 'Politica de Privacidade' }
            ]
        };
    },
    methods: {
        navegar(rota) {
            if (rota) {
                this.$router.push(rota);
            }
        },
        voltarAoTopo() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    },
    mounted() {
        this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
    }
};
</script>

<style scoped>
.pagina-poliprivacidade {
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
    color: #6b7280;
    /* cinza padrão */
    transition: color 0.2s;
    font-size: 1.1rem;
}

.breadcrumb-link:hover {
    color: #f97316;
    text-decoration: underline;
}

.breadcrumb-laranja {
    color: #f97316 !important;
    font-weight: 900;
    font-size: 1.1rem;
}

.breadcrumb-home-img {
    margin-top: -5px;
}

.logo-container {
    display: flex;
    justify-content: center;
    margin-top: 15px;
    margin-bottom: 10px;
}

.logo {
    max-height: 250px;
    height: auto;
}

.conteudo-politica {
  width: 100%;
    max-width: 1400px;
    margin: 0 auto;
    padding: 1.5rem;
}

.titulo-politica {
    font-size: 1.2rem;
    font-weight: 700;
    color: #111827;
    border-left: 4px solid #f97316;
    padding-left: 8px;
    margin-top: 2rem;
    margin-bottom: 1rem;
}

.introducao-politica {
    font-size: 1.4rem;
    font-weight: bold;
    color: #111827;
    margin-bottom: 1.5rem;
    line-height: 1.6;
}

.paragrafo-politica {
    font-size: 1.05rem;
    color: #4b5563;
    line-height: 1.7;
    margin-bottom: 1.3rem;
}

.titulo_topicos_politica {
    font-size: 1.2rem;
    font-weight: 700;
    color: #4b5563;
    border-left: 4px solid #f97316;
    padding-left: 8px;
    margin-top: 2rem;
    margin-bottom: 1rem;
}

.topicos_politica {
    color: #4b5563;
}

.lista-politica li {
    color: #4b5563;
}

.voltar-topo {
  position: fixed;
  left: 50%;
  bottom: 3%;                
  transform: translateX(-50%);
  z-index: 3000;
  cursor: pointer;
}

.icone-voltar-topo {
  width: 40px;
  height: 40px;
}

</style>