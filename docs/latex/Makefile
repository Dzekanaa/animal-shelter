.PHONY: all build build-v clean view plantuml help check-deps

MAIN_FILE = main
OUTPUT_DIR = build
UML_DIR = ../uml
FIGURES_DIR = figures

help:
	@echo "Komande:"
	@echo "  make all       - Generiši PNG + PDF"
	@echo "  make build     - Samo PDF"
	@echo "  make plantuml  - Generiši sve dijagrame (osim use-case)"
	@echo "  make view      - Otvori PDF"
	@echo "  make clean     - Obriši sve"

check-deps:
	@command -v pdflatex >/dev/null 2>&1 && echo "✓ pdflatex" || echo "✗ pdflatex"
	@command -v plantuml >/dev/null 2>&1 && echo "✓ plantuml" || echo "✗ plantuml"

build:
	@mkdir -p $(OUTPUT_DIR)
	pdflatex -interaction=nonstopmode -output-directory=$(OUTPUT_DIR) $(MAIN_FILE).tex > /dev/null
	@echo "✓ PDF: $(OUTPUT_DIR)/$(MAIN_FILE).pdf"

build-v:
	@mkdir -p $(OUTPUT_DIR)
	pdflatex -interaction=nonstopmode -output-directory=$(OUTPUT_DIR) $(MAIN_FILE).tex

plantuml:
	@mkdir -p $(FIGURES_DIR)
	@echo "🔍 Tražim dijagrame (osim use-case)..."
	@find $(UML_DIR) -name "*.puml" -type f ! -name "01-use-case.puml" | while read file; do \
		echo "   📐 Procesiram: $$file"; \
		plantuml -png "$$file"; \
		base=$$(basename "$$file" .puml); \
		mv "$$(dirname "$$file")/$$base.png" $(FIGURES_DIR)/; \
		echo "      -> $(FIGURES_DIR)/$$base.png"; \
	done
	@echo "✓ Svi dijagrami generisani u $(FIGURES_DIR)/"

all: plantuml build
	@echo "✓ GOTOVO!"

view: build
	@if command -v xdg-open >/dev/null 2>&1; then \
		xdg-open $(OUTPUT_DIR)/$(MAIN_FILE).pdf; \
	elif command -v open >/dev/null 2>&1; then \
		open $(OUTPUT_DIR)/$(MAIN_FILE).pdf; \
	else \
		echo "Otvori ručno: $(OUTPUT_DIR)/$(MAIN_FILE).pdf"; \
	fi

clean:
	rm -rf $(OUTPUT_DIR)
	rm -f $(FIGURES_DIR)/*.png
	rm -f *.pdf *.aux *.log *.out *.toc *.lof *.bbl *.blg
	@echo "Done"
