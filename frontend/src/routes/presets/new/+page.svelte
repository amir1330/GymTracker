<script>
  import { goto } from '$app/navigation';
  let name='';
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Presets',{method:'POST',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({name})});
    if(r.ok) goto('/presets');
  }
</script>
<h1>new preset</h1>
<form on:submit={submit}><input bind:value={name} placeholder="name" required /><button>save</button></form>
