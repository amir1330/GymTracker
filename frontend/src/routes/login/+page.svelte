<script>
  import { goto } from '$app/navigation';
  let email='', password='', err='';
  async function submit(e){
    e.preventDefault(); err='';
    const r = await fetch('/api/Auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({email,password})});
    const j = await r.json().catch(()=>({}));
    if(!r.ok){ err=j.message||'invalid'; return; }
    localStorage.setItem('jwt', j.token); goto('/progress');
  }
</script>
<div class="auth-container"><div class="auth-box">
<h2>login</h2>
<form on:submit={submit}>
  <div class="form-group"><label>email</label><input type="email" bind:value={email} placeholder="email" required /></div>
  <div class="form-group"><label>password</label><input bind:value={password} type="password" placeholder="password" required /></div>
  {#if err}<p class="error">{err}</p>{/if}
  <div class="form-actions"><button class="btn btn-primary">enter</button></div>
</form>
<p class="switch-link"><a href="/register">register</a></p>
</div></div>
