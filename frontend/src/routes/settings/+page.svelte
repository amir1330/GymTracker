<script>
  import { onMount } from 'svelte';
  import { t, setLang } from '$lib/i18n/index.js';
  let email='', theme='dark', language='en', success='';
  onMount(async ()=>{
    const tok=localStorage.getItem('jwt');
    const r=await fetch('/api/User/settings',{headers:{Authorization:'Bearer '+tok}});
    if(r.ok){ const s=await r.json(); email=s.Email||s.email||''; }
    const st=localStorage.getItem('theme'); if(st) theme=st;
    const lg=localStorage.getItem('lang'); if(lg) language=lg;
  });
  function setTheme(v){ theme=v; localStorage.setItem('theme',v); document.documentElement.setAttribute('data-theme', v==='light'?'light':''); }
  function setLanguage(v){ language=v; localStorage.setItem('lang',v); setLang(v); }
  function logout(){ localStorage.removeItem('jwt'); location.href='/login'; }
</script>
<div class="settings">
  <h2>{t('settings.title')}</h2>
  <div class="settings-section"><h3>{t('settings.profile')}</h3><div class="profile-info"><p><strong>{t('settings.email')}:</strong> {email}</p></div></div>
  <div class="settings-section"><h3>{t('settings.appearance')}</h3><div class="form-group"><label>{t('settings.theme')}</label><div class="theme-toggle"><button on:click={()=>setTheme('auto')} class="btn" class:btn-primary={theme==='auto'}>{t('settings.themeAuto')}</button> <button on:click={()=>setTheme('dark')} class="btn" class:btn-primary={theme==='dark'}>{t('settings.themeDark')}</button> <button on:click={()=>setTheme('light')} class="btn" class:btn-primary={theme==='light'}>{t('settings.themeLight')}</button></div></div></div>
  <div class="settings-section"><h3>{t('settings.language')}</h3><div class="form-group"><select bind:value={language} on:change={(e)=>setLanguage(e.target.value)} name="language"><option value="kz">{t('settings.languageKz')}</option><option value="ru">{t('settings.languageRu')}</option><option value="en">{t('settings.languageEn')}</option></select></div></div>
  <div class="settings-section"><h3>{t('settings.account')}</h3><button on:click={logout} class="btn btn-danger">{t('settings.logout')}</button></div>
  {#if success}<div class="success">{success}</div>{/if}
</div>
