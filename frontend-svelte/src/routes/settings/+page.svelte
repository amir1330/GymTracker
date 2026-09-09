<script>
  import { onMount } from 'svelte';
  let s={};
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/User/settings',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) s=await r.json();
  });
  async function save(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    await fetch('/api/User/settings',{method:'PUT',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify(s)});
  }
</script>
<h1>settings</h1>
<form on:submit={save}><button>save</button></form>
<pre>{JSON.stringify(s)}</pre>
