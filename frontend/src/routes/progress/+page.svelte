<script>
  import { onMount } from 'svelte';
  import ChartTile from '$lib/components/ChartTile.svelte';
  let charts=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Dashboard',{headers:{Authorization:'Bearer '+t}});
    if(!r.ok) return;
    const list=await r.json();
    charts=await Promise.all(list.map(async c=>{
      const d=await fetch('/api/Stats/chart-data',{method:'POST',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({period:c.Period||c.period||'30d',metric:c.Metric||c.metric||'volume',exerciseId:c.ExerciseId||c.exerciseId||null})}).then(x=>x.json()).catch(()=>({points:[]}));
      return {label:c.Label||c.label,points:d.points||[]};
    }));
  });
</script>
<h1>progress</h1>
{#each charts as c}<ChartTile label={c.label} points={c.points} />{/each}
{#if !charts.length}<p>no charts yet</p>{/if}
