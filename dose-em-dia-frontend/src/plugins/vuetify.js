// src/plugins/vuetify.js
import 'vuetify/styles';
import { createVuetify } from 'vuetify';
import * as components from 'vuetify/components';
import * as directives from 'vuetify/directives';
import '@mdi/font/css/materialdesignicons.css';
import { VDateInput } from 'vuetify/labs/VDateInput'
import { pt } from 'vuetify/locale' 

export default createVuetify({
  components: {
    ...components, 
    VDateInput,   
  },
  directives,
  icons: {
    defaultSet: 'mdi',
  },
   locale: {
    locale: 'pt',       
    fallback: 'pt',     
    messages: { pt },   
  },
})
