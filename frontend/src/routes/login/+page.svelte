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
<h1>login</h1>
<form on:submit={submit}>
  <input bind:value={email} placeholder="email" required />
  <input bind:value={password} type="password" placeholder="password" required />
  <button>enter</button>
  <p style="color:red">{err}</p>
</form>
<a href="/register">register</a>
