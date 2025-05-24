import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./assets/styles/global.css";
import 'mdb-ui-kit/css/mdb.min.css';
import '@fortawesome/fontawesome-free/css/all.min.css';
import 'mdb-ui-kit/js/mdb.min.js';
import { loadFonts } from './plugins/webfontloader'; // ou onde ele estiver
import vuetify from './plugins/vuetify'; // ou de onde você configurou

loadFonts();

const app = createApp(App);
app.use(router);
app.use(vuetify);
app.mount("#app");