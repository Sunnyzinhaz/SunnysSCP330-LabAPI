# SunnysSCP330 — migração LabAPI v2.2 (EM DESENVOLVIMENTO)

**ESTE PROJETO NÃO FOI COMPILADO NEM VALIDADO NO SERVIDOR. NÃO INSTALAR COMO RELEASE.**

## Origem

Arquivos originais da versão EXILED v1.2.6 preservados na pasta `REFERENCIA-ORIGINAL-EXILED-v1.2.6`. O código da pasta `SunnysSCP330` utiliza LabAPI, sem referência ao EXILED.

## Implementado no código (sujeito a validação)

- Esqueleto do plugin LabAPI com configurações INI editáveis.
- Watcher de alterações na bolsa SCP330. *Atenção*: ao observar apenas remoções, pode confundir descarte de doces e consumo. O evento nativo de consumo e o Harmony patch ainda precisam de implementação.
- 30 resultados da roleta do preto representados em código e com chaves booleanas individuais; muitas ações ainda possuem chamadas de reflexão não verificadas e algumas não reproduzem a mecânica original exatamente.
- Black Tantrum; tentativa de utilizar função nativa de poça; sem fallback validado.
- Gravidade Green extrema ajustável. Black Float usa gravidade, sem mudar `Position` neste resultado.
- Orange tenta `HauntedCandyOrange` sem LightToy; Evil tenta `HauntedCandyEvil`.
- Brown tenta a rotina nativa, com fallback para Tantrum quando ela não é localizada.

## Limitações importantes

- A migração funcional integral da v1.2.6 **não está concluída**; detalhes de Evil, Orange, Brown, Rainbow, efeitos da roleta, capivara, papel, inventário e RueI continuam incompletos.
- `Native` contém reflection de compatibilidade, mas não houve validação com as DLLs oficiais do servidor correspondente.
- Eventos de pickup, descarte e consumo precisam ser reconciliados com a versão exata de LabAPI.
- RuEI não está integrado de forma equivalente ao original.
- Sem compilação e teste em servidor, **não há garantia de funcionamento**.

## Rider

1. Extraia o ZIP numa pasta local no Windows.
2. Abra `SunnysSCP330/SunnysSCP330.csproj` pelo Rider (não apenas a página do GitHub).
3. Configure variável de ambiente `SL_REFERENCES` apontando para a pasta que contenha LabApi.dll, Assembly-CSharp.dll e módulos Unity do servidor.
4. Build > Build Solution. Pode haver erros de API: a compilação **não foi validada**.

A recomendação é não remover a versão EXILED do seu servidor até que a LabAPI esteja validada.
