<script>
  import { goto } from '$app/navigation';
  let name='', muscleGroup='', isDuration=false;
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{method:'POST',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({name,muscleGroup,isDuration})});
    if(r.ok) goto('/exercises');
  }
</script>
<h1>new exercise</h1>
<form on:submit={submit}>
  <input bind:value={name} placeholder="name" required maxlength="100" />
  <input bind:value={muscleGroup} placeholder="muscle group" required />
  <label><input type="checkbox" bind:checked={isDuration} /> duration-based</label>
  <button>save</button>
</form>
