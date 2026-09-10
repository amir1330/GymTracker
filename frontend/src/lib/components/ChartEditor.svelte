<script>
  import { t } from '$lib/i18n/index.js';
  export let chart=null, exercises=[], open=false;
  export let onsave=()=>{}, oncancel=()=>{};
  let label='', metric='volume', period='30d', chartType='line', exerciseId=null, preview={points:[]}, loading=false;
  const metrics=[
    {value:'weight',needsExercise:true},{value:'volume',needsExercise:false},
    {value:'duration',needsExercise:true},{value:'bodyWeight',needsExercise:false},
    {value:'frequency',needsExercise:false}
  ];
  const periods=['7d','30d','90d','180d','365d','all'];
  $: showExercise=(metrics.find(m=>m.value===metric)?.needsExercise)||false;
  $: if(chart){ label=chart.Label||chart.label||''; metric=chart.Metric||chart.metric||'volume'; period=chart.Period||chart.period||'30d'; }
  async function previewLoad(){
    loading=true;
    const tok=localStorage.getItem('jwt');
    const r=await fetch('/api/Stats/chart-data',{method:'POST',headers:{Authorization:'Bearer '+tok,'Content-Type':'application/json'},body:JSON.stringify({period,metric,exerciseId})});
    if(r.ok) preview=await r.json(); loading=false;
  }
  async function save(){
    const tok=localStorage.getItem('jwt');
    const body={label,metric,period,chartType,exerciseId};
    if(chart){ await fetch(`/api/Dashboard/${chart.Id||chart.id}`,{method:'PUT',headers:{Authorization:'Bearer '+tok,'Content-Type':'application/json'},body:JSON.stringify(body)}); }
    else { await fetch('/api/Dashboard',{method:'POST',headers:{Authorization:'Bearer '+tok,'Content-Type':'application/json'},body:JSON.stringify(body)}); }
    onsave();
  }
</script>
{#if open}
<div class="modal-overlay" on:click={oncancel}>
  <div class="modal-content" on:click|stopPropagation>
    <div class="modal-header"><h2>{chart?'edit':'add'}</h2><button class="btn-close" on:click={oncancel}>×</button></div>
    <div class="modal-body">
      <div class="form-group"><label>label</label><input type="text" bind:value={label} on:input={previewLoad} /></div>
      <div class="form-group"><label>metric</label><div class="radio-group">{#each metrics as m}<label class="radio-label"><input type="radio" value={m.value} bind:group={metric} name="metric" on:change={previewLoad} />{m.value}</label>{/each}</div></div>
      {#if showExercise}<div class="form-group"><label>exercise</label><select bind:value={exerciseId} on:change={previewLoad}><option value={null}>all</option>{#each exercises as e}<option value={e.Id||e.id}>{e.name||e.Name}</option>{/each}</select></div>{/if}
      <div class="form-group"><label>period</label><div class="radio-group">{#each periods as p}<label class="radio-label"><input type="radio" value={p} bind:group={period} name="period" on:change={previewLoad} />{p}</label>{/each}</div></div>
      <div class="form-group"><label>type</label><div class="radio-group"><label class="radio-label"><input type="radio" value="line" bind:group={chartType} name="ct" />line</label><label class="radio-label"><input type="radio" value="bar" bind:group={chartType} name="ct" />bar</label></div></div>
      {#if loading}<p>loading...</p>{:else}<p class="muted">{preview.points?.length||0} pts{#if preview.summary} — {preview.summary.current} ({preview.summary.change}){/if}</p>{/if}
    </div>
    <div class="modal-actions"><button class="btn btn-secondary" on:click={oncancel}>cancel</button><button class="btn btn-primary" on:click={save} disabled={!label.trim()||(showExercise&&!exerciseId)}>save</button></div>
  </div>
</div>
{/if}
