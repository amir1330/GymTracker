<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  let name='', muscleGroup='';
  const id=$page.params.id;
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch(`/api/Exercises/${id}`,{headers:{Authorization:'Bearer '+t}});
    if(r.ok){ const e=await r.json(); name=e.Name||e.name||''; muscleGroup=e.MuscleGroup||e.muscleGroup||''; }
  });
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Exercises/${id}`,{method:'PUT',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({name,muscleGroup})});
    goto('/exercises');
  }
</script>
<h1>edit exercise {id}</h1>
<form on:submit={submit}>
  <input bind:value={name} required /><input bind:value={muscleGroup} required />
  <button>save</button>
</form>
