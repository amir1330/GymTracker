<script>
  import { onMount } from 'svelte';
  import { t } from '$lib/i18n/index.js';
  import ChartTile from '$lib/components/ChartTile.svelte';
  let charts=[], loading=true, editorOpen=false, editing=null;
  async function load(){
    const tok=localStorage.getItem('jwt');
    const r=await fetch('/api/Dashboard',{headers:{Authorization:'Bearer '+tok}});
    if(!r.ok){ loading=false; return; }
    const list=await r.json();
    charts=await Promise.all(list.map(async c=>{
      const d=await fetch('/api/Stats/chart-data',{method:'POST',headers:{Authorization:'Bearer '+tok,'Content-Type':'application/json'},body:JSON.stringify({period:c.Period||c.period||'30d',metric:c.Metric||c.metric||'volume',exerciseId:c.ExerciseId||c.exerciseId||null})}).then(x=>x.json()).catch(()=>({points:[]})));
      return {...c,points:d.points||[]};
    }));
    loading=false;
  }
  onMount(load);
  async function delChart(c){
    const tok=localStorage.getItem('jwt');
    await fetch(`/api/Dashboard/${c.Id||c.id}`,{method:'DELETE',headers:{Authorization:'Bearer '+tok}});
    load();
  }
  async function move(c,dir){
    const tok=localStorage.getItem('jwt');
    const ids=charts.map(x=>x.Id||x.id);
    const i=ids.indexOf(c.Id||c.id);
    const j=i+dir; if(j<0||j>=ids.length) return;
    [ids[i],ids[j]]=[ids[j],ids[i]];
    await fetch('/api/Dashboard/reorder',{method:'PUT',headers:{Authorization:'Bearer '+tok,'Content-Type':'application/json'},body:JSON.stringify(ids.map((id,pos)=>({id,position:pos})))});
    load();
  }
</script>
<div class="progress-page">
  <div class="header"><h2>{t('progress.title')}</h2><a class="btn btn-primary" href="/progress/new">{t('progress.addChart')}</a></div>
  {#if loading}<p>{t('common.loading')}</p>
  {:else if !charts.length}<div class="empty-state"><p class="empty">{t('progress.empty')}</p></div>
  {:else}<div class="dashboard-grid">
  {#each charts as c,i}
    <div class="chart-wrapper"><ChartTile label={c.Label||c.label} points={c.points} />
      <div class="chart-controls"><button class="btn btn-small" on:click={()=>move(c,-1)} disabled={i===0}>{t('progress.moveUp')}</button> <button class="btn btn-small" on:click={()=>move(c,1)} disabled={i===charts.length-1}>{t('progress.moveDown')}</button> <button class="btn btn-small btn-danger" on:click={()=>delChart(c)}>{t('progress.delete')}</button></div>
    </div>
  {/each}
  </div>{/if}
</div>
