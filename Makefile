.PHONY: setup build test clean valida-consistencia

# Prepara o clone: ativa o hook que impede assinatura de coautoria de IA nos commits
setup:
	@git config core.hooksPath .githooks
	@echo "core.hooksPath = $$(git config --get core.hooksPath)"
	@echo "Hook de autoria ativo. Regras do repositorio: AGENTS.md"

# Compila toda a solução com o SDK selecionado em global.json
build:
	dotnet build

# Executa testes unitarios, defensivos e de contrato
test:
	dotnet test

# Verifica o histórico disponível no clone com as regras do hook
valida-consistencia:
	@python3 .githooks/commit-msg --check-history

# Limpeza de binarios e artefatos intermediarios
clean:
	find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true
