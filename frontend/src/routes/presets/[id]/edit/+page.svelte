<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  let name='';
  const id=$page.params.id;
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch(`/api/Presets/${id}`,{headers:{Authorization:'Bearer '+t}});
    if(r.ok){ const p=await r.json(); name=p.Name||p.name||''; }
  });
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Presets/${id}`,{method:'PUT',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({name})});
    goto('/presets');
  }
</script>
<h1>edit preset {id}</h1>
<form on:submit={submit}><input bind:value={name} required /><button>save</button></form>
