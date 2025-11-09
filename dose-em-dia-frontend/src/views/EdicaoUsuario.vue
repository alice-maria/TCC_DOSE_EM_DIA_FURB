<template>
  <v-container fluid class="pa-0">
    <div class="pagina-edituser">
      <!-- Cabeçalho -->
      <div class="header">
        <h1 class="titulo" @click="$router.push('/home')">Dose em dia</h1>
        <div class="usuario">
          <UsuarioMenu />
        </div>
      </div>

      <!-- Breadcrumbs -->
      <v-breadcrumbs :items="breadcrumbs" class="meus-breadcrumbs px-6">
        <template v-slot:item="{ item, index }">
          <span :class="[
            'breadcrumb-link',
            index === breadcrumbs.length - 1 ? 'breadcrumb-laranja' : ''
          ]" @click="item.to && navegar(item.to)" style="cursor: pointer;">
            <img v-if="index === 0" src="@/assets/icons/home.svg" class="breadcrumb-home-img" aria-hidden="true" />
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <p class="px-5 subtitulo">Informações sobre seu perfil e dados cadastrais.</p>

      <div class="linha-dado">
        <label>Nome:</label>
        <div>
          <span v-if="!editando">{{ usuario.nome }}</span>
          <input v-else v-model="form.nome" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>E-mail:</label>
        <div>
          <span v-if="!editando">{{ usuario.email }}</span>
          <input v-else v-model="form.email" type="email" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Telefone:</label>
        <div>
          <span v-if="!editando">{{ formatarTelefone(usuario.telefone) }}</span>
          <input v-else v-mask="'(##) #####-####'" v-model="form.telefone" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>CPF:</label>
        <div>
          <span v-if="!editando">{{ formatarCPF(usuario.cpf) }}</span>
          <input v-else v-model="form.cpf" type="text" class="form-control" disabled>
          <v-tooltip activator="parent" location="top">
            Este campo não é editável.
          </v-tooltip>
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Data de Nascimento:</label>
        <div>
          <span v-if="!editando">{{ formatarData(usuario.dataNascimento) }}</span>
          <input v-else v-model="form.dataNascimento" type="date" class="form-control" disabled>
          <v-tooltip activator="parent" location="top">
            Este campo não é editável.
          </v-tooltip>
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Sexo:</label>
        <div>
          <span v-if="!editando">{{ usuario.sexo || 'Não informado' }}</span>
          <input v-else v-model="form.sexo" type="text" class="form-control" disabled>
          <v-tooltip activator="parent" location="top">
            Este campo não é editável.
          </v-tooltip>
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>CEP:</label>
        <div>
          <span v-if="!editando">{{ cepView || 'Não informado' }}</span>
          <input v-else v-mask="'#####-###'" v-model="form.endereco.cep" type="text" class="form-control"
            @blur="buscarCep">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Endereço:</label>
        <div>
          <span v-if="!editando">{{ enderecoView || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.logradouro" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Número:</label>
        <div>
          <span v-if="!editando">{{ usuario.endereco?.numero || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.numero" type="text" class="form-control">
        </div>
      </div>
      <v-divider />


      <div class="linha-dado">
        <label>Bairro:</label>
        <div>
          <span v-if="!editando">{{ bairroView || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.bairro" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Cidade:</label>
        <div>
          <span v-if="!editando">{{ cidadeView || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.cidade" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="linha-dado">
        <label>Estado:</label>
        <div>
          <span v-if="!editando">{{ estadoView || 'Não informado' }}</span>
          <input v-else v-model="form.endereco.estado" type="text" class="form-control">
        </div>
      </div>
      <v-divider />

      <div class="text-center mt-4">
        <v-btn v-if="!editando" color="orange" class="botao-preenchido" @click="editando = true">
          Editar dados
        </v-btn>

        <div v-else class="d-flex justify-content-center gap-3">
          <v-btn class="botao-tonal" @click="cancelar">
            Cancelar
          </v-btn>
          <v-btn color="orange" class="botao-preenchido" @click="confirmarSalvar = true">
            Salvar
          </v-btn>
        </div>
      </div>
    </div>
  </v-container>
  <v-dialog v-model="confirmarSalvar" max-width="400" persistent>
    <v-card class="popup-confirmacao">
      <v-card-text class="texto-confirmacao">
        Você confirma a alteração dos dados?
      </v-card-text>
      <div class="botoes-popup">
        <v-btn class="btn-cancelar" @click="confirmarSalvar = false">Cancelar</v-btn>
        <v-btn class="btn-confirmar" @click="confirmarSalvar = false; salvar()">Confirmar</v-btn>
      </div>
    </v-card>
  </v-dialog>
  <!-- POPUP DE SUCESSO -->
  <v-dialog v-model="dialogSucesso" max-width="400" persistent>
    <v-card class="popup-sucesso">
      <v-card-text class="texto-sucesso">
        Informações alteradas com sucesso!
      </v-card-text>
      <v-card-actions class="botoes-popup">
        <v-btn class="btn-popupok" @click="$router.push('/informacoes-cadastrais')">Ok</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
  <!-- POPUP DE ERRO -->
  <v-dialog v-model="mostrarErro" max-width="400">
    <v-card>
      <v-alert type="error" color="red-darken-2" icon="mdi-alert-circle" class="pa-5" border="start" elevation="2"
        title="Erro ao salvar">
        {{ mensagem }}
      </v-alert>
      <v-card-actions class="justify-end">
        <v-btn color="red-darken-2" variant="flat" @click="mostrarErro = false">OK</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
import axios from "axios";
import UsuarioMenu from "@/views/UsuarioMenu.vue";

const baseURL = process.env.VUE_APP_API_BASE_URL || "https://doseemdiabackend-production.up.railway.app";

export const api = axios.create({
  baseURL: baseURL.replace(/\/+$/, ""),
  timeout: 20000,
});

export default {
  name: "EdicaoUsuario",
  components: { UsuarioMenu },

  data() {
    return {
      usuario: {},
      form: {
        nome: "",
        email: "",
        telefone: "",
        cpf: "",
        dataNascimento: "",
        sexo: "",
        cep: "",
        endereco: { cep: "", logradouro: "", numero: "", bairro: "", cidade: "", estado: "" },
      },
      editando: false,
      erro: "",
      mostrarErro: false,
      confirmarSalvar: false,
      dialogSucesso: false,
      cidadePreview: "",
      estadoPreview: "",
      cepAplicado: false,
      buscandoCep: false,
      breadcrumbs: [
        { text: "Início", to: "/home", icon: "mdi-home" },
        { text: "Configurações", to: "/configuracoes" },
        { text: "Informações Cadastrais" },
      ],
    };
  },

  methods: {
    navegar(rota) {
      if (rota) this.$router.push(rota);
    },

    editar() {
      this.editando = true;
      if (!this.form.endereco.cep) {
        this.form.endereco.cep = this.usuario?.endereco?.cep?.codigo || "";
      }
      this.cidadePreview = "";
      this.estadoPreview = "";
      this.cepAplicado = false;
    },

    formatarData(data) {
      if (!data) return "";
      const d = new Date(data);
      if (Number.isNaN(d.getTime())) return "";
      return d.toLocaleDateString("pt-BR");
    },

    async carregarUsuario() {
      const cpf = localStorage.getItem("usuarioCpf");
      if (!cpf) return;

      try {
        const { data } = await api.get(`/api/usuario/buscarPorCpf/${cpf}`);
        this.usuario = data;

        const dataFormatada =
          data?.dataNascimento
            ? (String(data.dataNascimento).includes("T") ? data.dataNascimento.split("T")[0] : data.dataNascimento)
            : "";

        this.form = {
          nome: data?.nome || "",
          email: data?.email || "",
          telefone: data?.telefone || "",
          cpf: data?.cpf || "",
          dataNascimento: dataFormatada || "",
          sexo: data?.sexo || "",
          endereco: {
            cep: data?.endereco?.cep?.codigo || "",
            logradouro: data?.endereco?.logradouro || "",
            numero: data?.endereco?.numero || "",
            bairro: data?.endereco?.cep?.bairro || data?.endereco?.bairro || "",
            cidade: data?.endereco?.cep?.cidade?.nome || "",
            estado: data?.endereco?.cep?.cidade?.estado?.uf || ""
          }
        };

        this.cidadePreview = "";
        this.estadoPreview = "";
        this.cepAplicado = false;
        this.cepErro = "";

      } catch (error) {
        console.error(error);
        this.erro = "Erro ao carregar dados do usuário.";
        this.mostrarErro = true;
      }
    },

    async salvar() {
      this.form.endereco = {
        cep: this.form.endereco?.cep ?? "",
        logradouro: this.form.endereco?.logradouro ?? "",
        numero: this.form.endereco?.numero ?? "",
        bairro: this.form.endereco?.bairro ?? "",
        cidade: this.form.endereco?.cidade ?? "",
        estado: this.form.endereco?.estado ?? ""
      };

      const trim = (v) => (v ?? "").toString().trim();
      const soDigitos = (v) => trim(v).replace(/\D/g, "");

      const isValidNome = (v) => trim(v).length >= 2;
      const isValidEmail = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(trim(v));
      const isValidTelefone = (v) => {
        const d = soDigitos(v);
        return d.length === 10 || d.length === 11;
      };
      const isValidCEP = (v) => soDigitos(v).length === 8;
      const isValidCidade = (v) => trim(v).length >= 2;
      const isValidUF = (v) => /^[A-Z]{2}$/i.test(trim(v));

      try {
        const id = Number(this.usuario?.idUser);
        if (!Number.isInteger(id)) {
          this.erro = "ID do usuário inválido.";
          this.mostrarErro = true;
          return;
        }

        const payload = {};
        const avisos = [];

        if (trim(this.form.nome) !== trim(this.usuario?.nome)) {
          if (isValidNome(this.form.nome)) payload.nome = trim(this.form.nome);
          else avisos.push("Nome não aplicado (mínimo 2 caracteres).");
        }

        if (trim(this.form.email).toLowerCase() !== trim(this.usuario?.email).toLowerCase()) {
          if (isValidEmail(this.form.email)) payload.email = trim(this.form.email);
          else avisos.push("E-mail não aplicado (formato inválido).");
        }

        if (soDigitos(this.form.telefone) !== soDigitos(this.usuario?.telefone)) {
          if (isValidTelefone(this.form.telefone)) payload.telefone = soDigitos(this.form.telefone);
          else avisos.push("Telefone não aplicado (use 10 ou 11 dígitos).");
        }

        if (trim(this.form.sexo) !== trim(this.usuario?.sexo)) {
          payload.sexo = trim(this.form.sexo);
        }

        const origEnd = {
          cep: this.usuario?.endereco?.cep?.codigo || "",
          logradouro: this.usuario?.endereco?.logradouro || "",
          numero: this.usuario?.endereco?.numero || "",
          bairro: this.usuario?.endereco?.cep?.bairro || this.usuario?.endereco?.bairro || "",
          cidade: this.usuario?.endereco?.cep?.cidade?.nome || this.usuario?.endereco?.cidade || "",
          estado: this.usuario?.endereco?.cep?.cidade?.estado?.uf || this.usuario?.endereco?.estado || "",
          existe: !!this.usuario?.endereco
        };

        const formEnd = {
          cep: trim(this.form.endereco.cep),
          logradouro: trim(this.form.endereco.logradouro),
          numero: trim(this.form.endereco.numero),
          bairro: trim(this.form.endereco.bairro),
          cidade: trim(this.form.endereco.cidade),
          estado: trim(this.form.endereco.estado)
        };

        const origCep8 = soDigitos(origEnd.cep);
        const formCep8 = soDigitos(formEnd.cep);

        const cepMudou = formEnd.cep !== "" && formCep8 !== origCep8;
        const logMudou = formEnd.logradouro !== "" && formEnd.logradouro !== origEnd.logradouro;
        const numMudou = formEnd.numero !== "" && String(formEnd.numero) !== String(origEnd.numero);
        const baiMudou = formEnd.bairro !== "" && formEnd.bairro !== origEnd.bairro;
        const cidMudou = formEnd.cidade !== "" && formEnd.cidade !== origEnd.cidade;
        const ufMudou = formEnd.estado !== "" && formEnd.estado.toUpperCase() !== String(origEnd.estado || "").toUpperCase();

        const endMudou = cepMudou || logMudou || numMudou || baiMudou || cidMudou || ufMudou;

        if (endMudou) {
          const cepParaEnviar = isValidCEP(formEnd.cep) ? formCep8 : origCep8;
          const cidadeParaEnviar = isValidCidade(formEnd.cidade) ? formEnd.cidade : origEnd.cidade;
          const ufParaEnviar = isValidUF(formEnd.estado)
            ? formEnd.estado.toUpperCase()
            : String(origEnd.estado || "").toUpperCase();

          const endPayload = {
            cep: cepParaEnviar,
            logradouro: formEnd.logradouro || origEnd.logradouro || "",
            numero: String(formEnd.numero || origEnd.numero || "").trim(),
            bairro: formEnd.bairro || origEnd.bairro || "",
            cidadeNome: cidadeParaEnviar || "",
            uf: (ufParaEnviar || "").toUpperCase()
          };

          if (!isValidCEP(endPayload.cep)) avisos.push("CEP inválido (use 8 dígitos).");
          if (!endPayload.numero) avisos.push("Número do endereço não informado.");

          if (avisos.length === 0) {
            payload.endereco = endPayload;
          }
        }

        if (Object.keys(payload).length === 0) {
          this.erro = avisos.length ? avisos.join(" ") : "Nenhuma alteração válida para salvar.";
          this.mostrarErro = true;
          return;
        }

        const resp = await api.patch(`/api/usuario/alterarDados/${id}`, payload, {
          headers: { "Content-Type": "application/json" }
        });

        if (resp.status === 200 || resp.status === 204) {
          this.editando = false;
          await this.carregarUsuario();
          this.dialogSucesso = true;
          setTimeout(() => (this.dialogSucesso = false), 1500);
        }
      } catch (error) {
        console.error("Erro ao salvar:", error?.response || error);
        this.erro =
          error?.response?.data?.message ??
          error?.response?.data ??
          "Erro ao salvar os dados. Tente novamente.";
        this.mostrarErro = true;
      }
    },

    cancelar() {
      this.editando = false;
      this.carregarUsuario();
      this.cidadePreview = "";
      this.estadoPreview = "";
      this.cepAplicado = false;
      this.carregarUsuario();
    },

    formatarTelefone(telefone) {
      if (!telefone) return "";
      const numeros = telefone.replace(/\D/g, "");
      if (numeros.length === 11) return `(${numeros.slice(0, 2)}) ${numeros.slice(2, 7)}-${numeros.slice(7)}`;
      if (numeros.length === 10) return `(${numeros.slice(0, 2)}) ${numeros.slice(2, 6)}-${numeros.slice(6)}`;
      return telefone;
    },

    formatarCPF(cpf) {
      if (!cpf) return "";
      const numeros = cpf.replace(/\D/g, "");
      return numeros.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
    },

    async buscarCep() {
      const cep8 = (this.form.endereco.cep || "").replace(/\D/g, "");
      this.cepAplicado = false;
      this.cidadePreview = "";
      this.estadoPreview = "";

      if (!cep8 || cep8.length !== 8) {
        this.cepErro = "CEP inválido. Use 8 dígitos.";
        return;
      }

      this.buscandoCep = true;
      try {
        const { data } = await axios.get(`https://viacep.com.br/ws/${cep8}/json/`);
        if (data?.erro) throw new Error("CEP não encontrado");

        this.form.endereco.logradouro = data.logradouro || this.form.endereco.logradouro || "";
        this.form.endereco.bairro = data.bairro || this.form.endereco.bairro || "";
        this.form.endereco.cidade = data.localidade || "";
        this.form.endereco.estado = data.uf || "";

        this.cidadePreview = data.localidade || "";
        this.estadoPreview = data.uf || "";
        this.cepAplicado = true;
      } catch (e) {
        this.cepErro = "CEP inválido ou serviço indisponível.";
        this.cidadePreview = "";
        this.estadoPreview = "";
        this.cepAplicado = false;
      } finally {
        this.buscandoCep = false;
      }
    },
  },

  mounted() {
    this.carregarUsuario();
    this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
  },

  computed: {
    cepView() {
      return this.form.endereco.cep
        || this.usuario?.endereco?.cep?.codigo
        || "";
    },
    enderecoView() {
      const log = this.form.endereco.logradouro || this.usuario?.endereco?.logradouro;
      return log || "";
    },
    bairroView() {
      return this.form.endereco.bairro || this.usuario?.endereco?.cep?.bairro || this.usuario?.endereco?.bairro
    },
    cidadeView() {
      return this.form?.endereco?.cidade || this.usuario?.endereco?.cep?.cidade?.nome || this.usuario?.endereco?.cidade || "";
    },
    estadoView() {
      return this.form?.endereco?.estado || this.usuario?.endereco?.cep?.cidade?.estado?.uf || this.usuario?.endereco?.estado || "";
    },
    paisView() {
      return this.usuario?.endereco?.cep?.cidade?.estado?.pais?.nome || "";
    }
  },
};
</script>

<style scoped>
.pagina-edituser {
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

.breadcrumb-link {
  color: #6b7280;
  transition: color 0.2s;
  font-size: 1.1rem;
}

.breadcrumb-link:hover {
  color: #f97316;
  text-decoration: underline;
}

.breadcrumb-home-img {
  margin-top: -5px;
}

.breadcrumb-laranja {
  color: #f97316 !important;
  font-weight: 900;
  font-size: 1.1rem;
}

.subtitulo {
  margin-top: -10px;
  font-size: 15px;
}

.linha-dado {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  font-size: 1rem;
  padding: 0.9rem;
  margin-left: 8px;
}

.linha-dado label {
  font-weight: bold;
  color: #000;
  flex: 1;
  min-width: 140px;
  margin: 0;
}

.linha-dado div {
  flex: 2;
  text-align: left;
  color: #333;
}

.v-alert {
  font-size: 1rem;
}

.v-card-actions .v-btn {
  font-weight: normal;
  border-radius: 20px;
  padding: 6px 16px;
}

.btn-orange,
.botao-preenchido,
.botao-tonal {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  line-height: 1.2;
  font-weight: bold;
  padding: 12px 24px;
  border-radius: 24px;
  text-transform: none;
}

.btn-orange {
  background-color: #f46c20;
  color: white;
  border: none;
}

.botao-preenchido {
  background-color: #f46c20 !important;
  color: white !important;
  box-shadow: 0px 3px 6px rgba(0, 0, 0, 0.15);
  margin-top: -50px;
}

.botao-tonal {
  background-color: #e0e0e0 !important;
  color: #212121 !important;
  font-weight: 600;
  margin-top: -50px;
}

.linha-dado input.form-control {
  height: 3.125rem;
  font-size: 1em;
  padding: 12px 16px;
  line-height: 1.5;
  border: 1.5px solid #ccc;
  margin-left: -90px;
}

.popup-confirmacao {
  background-color: #f97316;
  border-radius: 25px !important;
  padding: 20px;
  text-align: center;
}

.texto-confirmacao {
  color: white;
  font-weight: bold;
  font-size: 1.2rem;
  margin-bottom: 16px;
}

.botoes-popup {
  display: flex;
  justify-content: center;
  gap: 12px;
}

.btn-cancelar {
  background-color: #fda65b;
  color: white;
  font-weight: bold;
  border-radius: 999px;
  text-transform: none;
}

.btn-confirmar {
  background-color: white;
  color: #f97316;
  font-weight: bold;
  border-radius: 999px;
  text-transform: none;
}

.popup-sucesso {
  background-color: #f46c20;
  border-radius: 24px !important;
  padding: 40px 20px;
  width: 300px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
  text-align: center;
}

.texto-sucesso {
  color: white;
  font-weight: bold;
  font-size: 1.2rem;
  text-align: center;
}

.botoes-popup {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}

.btn-popupok {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 20px;
  width: 100%;
}

.btn-popupok button {
  background-color: #fff !important;
  color: #ff6600 !important;
  border: none;
  padding: 10px 30px;
  border-radius: 8px;
  font-weight: bold;
  cursor: pointer;
  text-transform: none;
  transition: 0.3s ease;
}
</style>