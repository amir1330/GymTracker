import en from './en.json';
import ru from './ru.json';
import kz from './kz.json';
const dicts = { en, ru, kz };
let lang = 'en';
export function setLang(l){ if(dicts[l]) lang=l; }
export function t(path){
  return path.split('.').reduce((o,k)=>(o&&o[k]!==undefined)?o[k]:path, dicts[lang]);
}
