# 🎮 BulletStorm
BulletStorm é um jogo **roguelike** desenvolvido com **MonoGame** (.NET 8), focado em ação rápida, progressão por fases e um sistema de pontuação baseado em performance. Enfrenta ondas de inimigos, sobrevive, melhora tuas habilidades e derrota o boss final.


## 🧠 Desenvolvedores
- Anthony Frade (31477)  
- Valezka Naia (31481)

## 🕹️ Controles

| Tecla | Ação      |
|-------|-----------|
| W     | Mover para cima |
| S     | Mover para baixo |
| A     | Mover para a esquerda |
| D     | Mover para a direita |
| E     | Habilidade Especial |
| F11   | Tela cheia |

---

## 📦 Estrutura geral do projeto

- Desenvolvido com **MonoGame (.NET 8)**.
- Arquitetura baseada em **estados** (`Menu`, `InputName`, `Playing`, `Scores`, `Credits`).
- Loop principal (`Game1`) gerencia a lógica, entrada de dados, desenho e transições.

---

## 🧭 Fluxo do jogo

1. **Menu inicial**
   - Interface desenhada manualmente com retângulos e texto.
   - Três opções: `Start`, `Scores`, `Créditos`.
   - Navegação por teclado e rato

2. **Input de nome**
   - O jogador insere um nome (máx. 12 caracteres).
   - Nome usado para salvar o score no final da sessão.

3. **Gameplay**
   - O jogador enfrenta várias **fases** com ondas de inimigos e dificuldade crescente.
   - Power-ups disponíveis em fases específicas (dano, vida, crit, velocidade).
   - O boss final aparece na última fase.

4. **Score**
   - Fórmula: `score = inimigos mortos * 100 - tempo (em segundos)`
   - Score mínimo: 0
   - Os resultados (nome, score, inimigos, tempo) são exibidos na tela de "Scores" (com persistência, através de um ficheiro .txt).

---

## 🛠️ Decisões de implementação

- **Simples e funcional**: Interface feita sem bibliotecas externas.
- **Separação e organização**: Classes distintas para `Player`, `Enemy`, `Weapon`, `LevelManager`, etc.
- **Sistema de estados** limpo e intuitivo usando enums (`GameState`, `Phase`).
- **Input de texto** básico (letras, números e espaços).
- **Pontuação justa** que recompensa desempenho.

---

## 🚀 Possíveis Melhorias Futuras

- Melhorar o HUD e GUI
- Animações e efeitos no menu.
- Sistema de upgrades persistentes.
- Personalização visual do jogador.
- Mais tipos de inimigos e fases/níveis
- Melhorar sprites
- Adicionar várias personagens com habilidades diferentes ou armas diferentes e possivel edição do personagem
- Construção de armas 

---

## 🧪 Como Jogar

1. Compila o projeto com .NET 8 e MonoGame instalado.
2. Executa o binário gerado ou roda direto da IDE (Visual Studio/JetBrains Rider).
3. Usa os controles para navegar e jogar.

---

## 🏁 Requisitos

- .NET 8 SDK  
- MonoGame Framework  
- Sistema Windows (Linux/Mac podem precisar de configurações extra)

---

## 📂 Organização do Código

```plaintext
├── BulletStorm/
│   ├── Game1.cs              # Loop principal do jogo
│   ├── States/               # Menu, Scores, Credits, Input
│   ├── Entities/             # Player, Enemy, Weapon, etc.
│   ├── Levels/               # LevelManager, Phase logic
│   ├── Content/              # Assets do jogo (imagens, sons, fontes)
│   └── Utils/                # Helpers e lógica de pontuação
```

---

> Este projeto foi desenvolvido como parte de um trabalho académico no curso de Engenharia e Desenvolvimento de Jogos (IPCA).
