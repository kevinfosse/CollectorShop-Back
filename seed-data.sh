#!/bin/bash
set -e

API="http://localhost:5000/api"

post() {
  curl -s -X POST "$1" -H "Content-Type: application/json" -d "$2"
}

get_id() {
  python3 -c "import sys,json; print(json.load(sys.stdin)['id'])"
}

echo "=== Creating missing categories ==="

CAT_RETRO=$(post "$API/categories" '{
  "name":"Jeux video retro","description":"Consoles et jeux video vintage et retro","slug":"retro-gaming",
  "imageUrl":"https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=400","displayOrder":4
}' | get_id)
echo "Retro Gaming: $CAT_RETRO"

CAT_VINYL=$(post "$API/categories" '{
  "name":"Vinyles","description":"Disques vinyles rares et editions limitees","slug":"vinyl",
  "imageUrl":"https://images.unsplash.com/photo-1539375665275-f9de415ef9ac?w=400","displayOrder":5
}' | get_id)
echo "Vinyl: $CAT_VINYL"

# Existing categories
CAT_FIGURINES="6efa80dd-467f-4fa9-bfab-197c66b5f9a1"
CAT_COMICS="9a2913f4-fbe5-4f0a-ba5a-873d68e6924c"
CAT_CARTES="b04e3561-3401-4232-b683-eb21d76985a0"

echo ""
echo "=== Creating brands ==="

BRAND_BANDAI=$(post "$API/brands" '{
  "name":"Bandai Namco","description":"Fabricant japonais de figurines et jouets","slug":"bandai",
  "logoUrl":"https://upload.wikimedia.org/wikipedia/commons/thumb/2/2d/Bandai_Namco_Holdings_logo.svg/200px-Bandai_Namco_Holdings_logo.svg.png",
  "websiteUrl":"https://www.bandai.com"
}' | get_id)
echo "Bandai: $BRAND_BANDAI"

BRAND_FUNKO=$(post "$API/brands" '{
  "name":"Funko","description":"Createur de figurines Pop! et objets de collection","slug":"funko",
  "logoUrl":"https://upload.wikimedia.org/wikipedia/commons/thumb/5/51/Funko_logo.svg/200px-Funko_logo.svg.png",
  "websiteUrl":"https://www.funko.com"
}' | get_id)
echo "Funko: $BRAND_FUNKO"

BRAND_POKEMON=$(post "$API/brands" '{
  "name":"The Pokemon Company","description":"Editeur officiel des cartes et produits Pokemon","slug":"pokemon-company",
  "logoUrl":"https://upload.wikimedia.org/wikipedia/commons/thumb/9/98/International_Pok%C3%A9mon_logo.svg/200px-International_Pok%C3%A9mon_logo.svg.png",
  "websiteUrl":"https://www.pokemon.com"
}' | get_id)
echo "Pokemon Company: $BRAND_POKEMON"

BRAND_NINTENDO=$(post "$API/brands" '{
  "name":"Nintendo","description":"Entreprise japonaise de jeux video et consoles","slug":"nintendo",
  "logoUrl":"https://upload.wikimedia.org/wikipedia/commons/thumb/0/0d/Nintendo.svg/200px-Nintendo.svg.png",
  "websiteUrl":"https://www.nintendo.com"
}' | get_id)
echo "Nintendo: $BRAND_NINTENDO"

BRAND_MARVEL=$(post "$API/brands" '{
  "name":"Marvel","description":"Univers Marvel - Comics et produits derives","slug":"marvel",
  "logoUrl":"https://upload.wikimedia.org/wikipedia/commons/thumb/b/b9/Marvel_Logo.svg/200px-Marvel_Logo.svg.png",
  "websiteUrl":"https://www.marvel.com"
}' | get_id)
echo "Marvel: $BRAND_MARVEL"

BRAND_SONY=$(post "$API/brands" '{
  "name":"Sony","description":"Geant de l electronique et du jeu video","slug":"sony",
  "logoUrl":"https://upload.wikimedia.org/wikipedia/commons/thumb/c/ca/Sony_logo.svg/200px-Sony_logo.svg.png",
  "websiteUrl":"https://www.sony.com"
}' | get_id)
echo "Sony: $BRAND_SONY"

echo ""
echo "=== Creating products ==="

# --- FIGURINES ---

post "$API/products" "{
  \"name\":\"Dragon Ball Z - Figurine Goku Super Saiyan\",
  \"description\":\"Figurine articulee de Goku en Super Saiyan, serie S.H.Figuarts. Hauteur 14cm avec accessoires et mains interchangeables. Edition speciale 35eme anniversaire.\",
  \"sku\":\"FIG-DBZ-GOKU-SSJ\",
  \"price\":89.99,\"currency\":\"EUR\",\"compareAtPrice\":109.99,
  \"stockQuantity\":15,\"condition\":0,\"weight\":0.3,\"dimensions\":\"14x10x8 cm\",
  \"categoryId\":\"$CAT_FIGURINES\",\"brandId\":\"$BRAND_BANDAI\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1608889825205-eebdb9fc5806?w=600\",\"altText\":\"Goku Super Saiyan figurine\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Hauteur\",\"value\":\"14 cm\"},{\"name\":\"Materiau\",\"value\":\"PVC & ABS\"},{\"name\":\"Serie\",\"value\":\"S.H.Figuarts\"}]
}" > /dev/null
echo "Created: Dragon Ball Z - Goku SSJ"

post "$API/products" "{
  \"name\":\"Funko Pop! Spider-Man No Way Home #913\",
  \"description\":\"Figurine Funko Pop! de Spider-Man en costume integre du film No Way Home. Vinyle, hauteur environ 10cm. Boite incluse en parfait etat.\",
  \"sku\":\"FIG-FUNKO-SPIDEY-913\",
  \"price\":14.99,\"currency\":\"EUR\",\"compareAtPrice\":19.99,
  \"stockQuantity\":42,\"condition\":0,\"weight\":0.15,\"dimensions\":\"10x7x7 cm\",
  \"categoryId\":\"$CAT_FIGURINES\",\"brandId\":\"$BRAND_FUNKO\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1611457194403-d3571b0653f0?w=600\",\"altText\":\"Funko Pop Spider-Man\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Numero\",\"value\":\"#913\"},{\"name\":\"Collection\",\"value\":\"Marvel\"},{\"name\":\"Materiau\",\"value\":\"Vinyle\"}]
}" > /dev/null
echo "Created: Funko Pop Spider-Man"

post "$API/products" "{
  \"name\":\"One Piece - Luffy Gear 5 Statue\",
  \"description\":\"Statue premium de Luffy en Gear 5 par Banpresto. Hauteur 25cm, finition detaillee avec base incluse. Edition limitee.\",
  \"sku\":\"FIG-OP-LUFFY-G5\",
  \"price\":149.99,\"currency\":\"EUR\",
  \"stockQuantity\":8,\"condition\":0,\"weight\":0.8,\"dimensions\":\"25x15x12 cm\",
  \"categoryId\":\"$CAT_FIGURINES\",\"brandId\":\"$BRAND_BANDAI\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1613771404784-3a5686aa2be3?w=600\",\"altText\":\"Luffy Gear 5 statue\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Hauteur\",\"value\":\"25 cm\"},{\"name\":\"Serie\",\"value\":\"Banpresto\"},{\"name\":\"Edition\",\"value\":\"Limitee\"}]
}" > /dev/null
echo "Created: One Piece Luffy Gear 5"

post "$API/products" "{
  \"name\":\"Naruto Shippuden - Itachi Uchiwa Figure\",
  \"description\":\"Figurine d Itachi Uchiwa en position de combat avec Susanoo partiel. Details peints a la main, base lumineuse LED incluse.\",
  \"sku\":\"FIG-NAR-ITACHI\",
  \"price\":74.50,\"currency\":\"EUR\",
  \"stockQuantity\":22,\"condition\":0,\"weight\":0.5,\"dimensions\":\"18x12x10 cm\",
  \"categoryId\":\"$CAT_FIGURINES\",\"brandId\":\"$BRAND_BANDAI\",\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1609372332255-611485350f25?w=600\",\"altText\":\"Itachi Uchiwa figurine\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Hauteur\",\"value\":\"18 cm\"},{\"name\":\"LED\",\"value\":\"Oui\"},{\"name\":\"Materiau\",\"value\":\"PVC\"}]
}" > /dev/null
echo "Created: Naruto - Itachi"

# --- COMICS ---

post "$API/products" "{
  \"name\":\"Amazing Spider-Man #300 - First Venom\",
  \"description\":\"Premiere apparition de Venom! Amazing Spider-Man numero 300 de 1988 par Todd McFarlane. Etat Near Mint (9.4). Protege sous pochette avec carton.\",
  \"sku\":\"COM-ASM-300\",
  \"price\":850.00,\"currency\":\"EUR\",\"compareAtPrice\":1100.00,
  \"stockQuantity\":2,\"condition\":2,\"weight\":0.2,\"dimensions\":\"26x17x0.5 cm\",
  \"categoryId\":\"$CAT_COMICS\",\"brandId\":\"$BRAND_MARVEL\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1612036782180-6f0b6cd846fe?w=600\",\"altText\":\"Amazing Spider-Man 300\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Annee\",\"value\":\"1988\"},{\"name\":\"Grade\",\"value\":\"9.4 NM\"},{\"name\":\"Artiste\",\"value\":\"Todd McFarlane\"}]
}" > /dev/null
echo "Created: Amazing Spider-Man #300"

post "$API/products" "{
  \"name\":\"X-Men #1 (1991) - Jim Lee Cover\",
  \"description\":\"X-Men numero 1 de 1991, couverture iconique de Jim Lee. Record de ventes de comics de tous les temps. Etat Very Fine.\",
  \"sku\":\"COM-XMEN-1-91\",
  \"price\":45.00,\"currency\":\"EUR\",
  \"stockQuantity\":5,\"condition\":2,\"weight\":0.15,\"dimensions\":\"26x17x0.3 cm\",
  \"categoryId\":\"$CAT_COMICS\",\"brandId\":\"$BRAND_MARVEL\",\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1601645191163-3fc0d5d64e35?w=600\",\"altText\":\"X-Men 1 Jim Lee\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Annee\",\"value\":\"1991\"},{\"name\":\"Grade\",\"value\":\"8.0 VF\"},{\"name\":\"Artiste\",\"value\":\"Jim Lee\"}]
}" > /dev/null
echo "Created: X-Men #1"

post "$API/products" "{
  \"name\":\"Manga Dragon Ball - Tome 1 Edition Originale\",
  \"description\":\"Premiere edition japonaise du tome 1 de Dragon Ball par Akira Toriyama (1985). Couverture en bon etat, pages jaunies mais lisibles.\",
  \"sku\":\"COM-DB-T1-JP\",
  \"price\":220.00,\"currency\":\"EUR\",
  \"stockQuantity\":1,\"condition\":3,\"weight\":0.2,\"dimensions\":\"18x12x1.5 cm\",
  \"categoryId\":\"$CAT_COMICS\",\"brandId\":\"$BRAND_BANDAI\",\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1618519764620-7403abdbdfe9?w=600\",\"altText\":\"Dragon Ball Tome 1 JP\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Langue\",\"value\":\"Japonais\"},{\"name\":\"Annee\",\"value\":\"1985\"},{\"name\":\"Auteur\",\"value\":\"Akira Toriyama\"}]
}" > /dev/null
echo "Created: Dragon Ball Tome 1 Edition Originale"

# --- CARTES A COLLECTIONNER ---

post "$API/products" "{
  \"name\":\"Carte Pokemon - Dracaufeu 1ere Edition Base Set\",
  \"description\":\"La carte la plus iconique de Pokemon! Dracaufeu holographique 1ere edition du set de base (1999). Etat PSA 7 Near Mint. Carte authentifiee et encapsulee.\",
  \"sku\":\"TCG-PKM-CHARIZARD-1ED\",
  \"price\":4500.00,\"currency\":\"EUR\",
  \"stockQuantity\":1,\"condition\":2,\"weight\":0.05,\"dimensions\":\"9x6 cm\",
  \"categoryId\":\"$CAT_CARTES\",\"brandId\":\"$BRAND_POKEMON\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1613771404784-3a5686aa2be3?w=600\",\"altText\":\"Charizard Base Set 1st Edition\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Grade PSA\",\"value\":\"7\"},{\"name\":\"Set\",\"value\":\"Base Set 1999\"},{\"name\":\"Rarete\",\"value\":\"Holographique rare\"}]
}" > /dev/null
echo "Created: Dracaufeu 1ere Edition"

post "$API/products" "{
  \"name\":\"Display Pokemon - Ecarlate et Violet 151\",
  \"description\":\"Boite de 36 boosters Pokemon Ecarlate et Violet 151 scellee. Contient les 151 Pokemon originaux en version moderne.\",
  \"sku\":\"TCG-PKM-EV151-DISPLAY\",
  \"price\":189.99,\"currency\":\"EUR\",\"compareAtPrice\":215.00,
  \"stockQuantity\":10,\"condition\":0,\"weight\":1.2,\"dimensions\":\"30x20x10 cm\",
  \"categoryId\":\"$CAT_CARTES\",\"brandId\":\"$BRAND_POKEMON\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1627856013091-fed6e4e30025?w=600\",\"altText\":\"Display Pokemon 151\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Nombre de boosters\",\"value\":\"36\"},{\"name\":\"Extension\",\"value\":\"Ecarlate et Violet 151\"},{\"name\":\"Langue\",\"value\":\"Francais\"}]
}" > /dev/null
echo "Created: Display Pokemon EV 151"

post "$API/products" "{
  \"name\":\"Lot 50 cartes Pokemon rares et holographiques\",
  \"description\":\"Lot de 50 cartes Pokemon toutes rares ou holographiques. Mix de differentes extensions, parfait pour completer une collection.\",
  \"sku\":\"TCG-PKM-LOT50\",
  \"price\":34.99,\"currency\":\"EUR\",
  \"stockQuantity\":25,\"condition\":1,\"weight\":0.15,\"dimensions\":\"10x7x3 cm\",
  \"categoryId\":\"$CAT_CARTES\",\"brandId\":\"$BRAND_POKEMON\",\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1606503153255-59d8b8b82176?w=600\",\"altText\":\"Lot cartes Pokemon\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Nombre de cartes\",\"value\":\"50\"},{\"name\":\"Type\",\"value\":\"Rares et Holo\"},{\"name\":\"Etat moyen\",\"value\":\"Excellent\"}]
}" > /dev/null
echo "Created: Lot 50 cartes Pokemon"

# --- JEUX VIDEO RETRO ---

post "$API/products" "{
  \"name\":\"Super Nintendo SNES - Console complete en boite\",
  \"description\":\"Console Super Nintendo en boite d origine avec 2 manettes, cables et notice. Testee et fonctionnelle. Boite en bon etat avec usure normale.\",
  \"sku\":\"RETRO-SNES-CIB\",
  \"price\":249.99,\"currency\":\"EUR\",
  \"stockQuantity\":3,\"condition\":3,\"weight\":2.5,\"dimensions\":\"35x25x15 cm\",
  \"categoryId\":\"$CAT_RETRO\",\"brandId\":\"$BRAND_NINTENDO\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600\",\"altText\":\"Super Nintendo SNES CIB\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Region\",\"value\":\"PAL\"},{\"name\":\"Inclus\",\"value\":\"Console, 2 manettes, cables, notice\"},{\"name\":\"Annee\",\"value\":\"1992\"}]
}" > /dev/null
echo "Created: Super Nintendo SNES CIB"

post "$API/products" "{
  \"name\":\"The Legend of Zelda: Ocarina of Time N64\",
  \"description\":\"Jeu The Legend of Zelda Ocarina of Time pour Nintendo 64. Cartouche seule en parfait etat de fonctionnement. Un classique intemporel!\",
  \"sku\":\"RETRO-N64-ZELDA-OOT\",
  \"price\":59.99,\"currency\":\"EUR\",
  \"stockQuantity\":7,\"condition\":2,\"weight\":0.1,\"dimensions\":\"12x8x2 cm\",
  \"categoryId\":\"$CAT_RETRO\",\"brandId\":\"$BRAND_NINTENDO\",\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1566577739112-5180d4bf9390?w=600\",\"altText\":\"Zelda Ocarina of Time N64\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Console\",\"value\":\"Nintendo 64\"},{\"name\":\"Region\",\"value\":\"PAL\"},{\"name\":\"Format\",\"value\":\"Cartouche seule\"}]
}" > /dev/null
echo "Created: Zelda Ocarina of Time N64"

post "$API/products" "{
  \"name\":\"PlayStation 1 - Console + Final Fantasy VII\",
  \"description\":\"Pack console PlayStation 1 grise avec le jeu Final Fantasy VII complet (3 disques + notice + boitier). Bundle ideal pour les fans de RPG retro.\",
  \"sku\":\"RETRO-PS1-FF7-BUNDLE\",
  \"price\":129.99,\"currency\":\"EUR\",
  \"stockQuantity\":2,\"condition\":3,\"weight\":3.0,\"dimensions\":\"40x30x15 cm\",
  \"categoryId\":\"$CAT_RETRO\",\"brandId\":\"$BRAND_SONY\",\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=600\",\"altText\":\"PS1 + FF7 Bundle\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Console\",\"value\":\"PlayStation 1\"},{\"name\":\"Jeu inclus\",\"value\":\"Final Fantasy VII\"},{\"name\":\"Region\",\"value\":\"PAL\"}]
}" > /dev/null
echo "Created: PS1 + Final Fantasy VII Bundle"

post "$API/products" "{
  \"name\":\"Game Boy Color - Edition Pikachu\",
  \"description\":\"Game Boy Color edition speciale Pikachu en excellent etat. Coque jaune avec Pikachu imprime. Fonctionne parfaitement, ecran sans rayures.\",
  \"sku\":\"RETRO-GBC-PIKACHU\",
  \"price\":169.99,\"currency\":\"EUR\",
  \"stockQuantity\":4,\"condition\":2,\"weight\":0.2,\"dimensions\":\"14x8x3 cm\",
  \"categoryId\":\"$CAT_RETRO\",\"brandId\":\"$BRAND_NINTENDO\",\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1531525645387-7f14be1bdbbd?w=600\",\"altText\":\"Game Boy Color Pikachu Edition\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Edition\",\"value\":\"Pikachu\"},{\"name\":\"Couleur\",\"value\":\"Jaune\"},{\"name\":\"Annee\",\"value\":\"1998\"}]
}" > /dev/null
echo "Created: Game Boy Color Pikachu"

# --- VINYLES ---

post "$API/products" "{
  \"name\":\"Pink Floyd - The Dark Side of the Moon (1973 Press)\",
  \"description\":\"Pressage original UK de 1973. Pochette gatefold avec posters et stickers originaux. Vinyle en etat VG+, pochette VG. Un incontournable!\",
  \"sku\":\"VIN-PF-DSOTM-73\",
  \"price\":320.00,\"currency\":\"EUR\",
  \"stockQuantity\":1,\"condition\":3,\"weight\":0.4,\"dimensions\":\"31x31x0.5 cm\",
  \"categoryId\":\"$CAT_VINYL\",\"brandId\":null,\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1539375665275-f9de415ef9ac?w=600\",\"altText\":\"Dark Side of the Moon vinyl\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Pressage\",\"value\":\"UK Original 1973\"},{\"name\":\"Etat vinyle\",\"value\":\"VG+\"},{\"name\":\"Etat pochette\",\"value\":\"VG\"}]
}" > /dev/null
echo "Created: Pink Floyd - Dark Side of the Moon"

post "$API/products" "{
  \"name\":\"Daft Punk - Random Access Memories (2LP)\",
  \"description\":\"Double vinyle 180g de Random Access Memories par Daft Punk. Edition originale de 2013, scellee et jamais ouverte. Pochette gatefold impeccable.\",
  \"sku\":\"VIN-DP-RAM-2LP\",
  \"price\":75.00,\"currency\":\"EUR\",\"compareAtPrice\":95.00,
  \"stockQuantity\":6,\"condition\":0,\"weight\":0.5,\"dimensions\":\"31x31x1 cm\",
  \"categoryId\":\"$CAT_VINYL\",\"brandId\":null,\"isFeatured\":true,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1614613535308-eb5fbd3d2c17?w=600\",\"altText\":\"Daft Punk RAM vinyl\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Format\",\"value\":\"2xLP 180g\"},{\"name\":\"Annee\",\"value\":\"2013\"},{\"name\":\"Etat\",\"value\":\"Scelle / Neuf\"}]
}" > /dev/null
echo "Created: Daft Punk - Random Access Memories"

post "$API/products" "{
  \"name\":\"Nirvana - Nevermind (LP Original 1991)\",
  \"description\":\"Pressage original US de Nevermind par Nirvana (DGC 1991). Vinyle en etat Excellent, pochette en tres bon etat. Inclut inner sleeve.\",
  \"sku\":\"VIN-NIR-NEVERMIND-91\",
  \"price\":195.00,\"currency\":\"EUR\",
  \"stockQuantity\":2,\"condition\":2,\"weight\":0.35,\"dimensions\":\"31x31x0.5 cm\",
  \"categoryId\":\"$CAT_VINYL\",\"brandId\":null,\"isFeatured\":false,
  \"images\":[{\"url\":\"https://images.unsplash.com/photo-1619983081563-430f63602796?w=600\",\"altText\":\"Nirvana Nevermind vinyl\",\"displayOrder\":1,\"isPrimary\":true}],
  \"attributes\":[{\"name\":\"Pressage\",\"value\":\"US Original 1991\"},{\"name\":\"Label\",\"value\":\"DGC Records\"},{\"name\":\"Etat vinyle\",\"value\":\"Excellent\"}]
}" > /dev/null
echo "Created: Nirvana - Nevermind"

echo ""
echo "=== Seeding complete! ==="
echo "Verifying..."
curl -s "$API/products?pageSize=50" | python3 -c "
import sys,json
data = json.load(sys.stdin)
print(f\"Total products: {data['totalCount']}\")
for item in data['items']:
    print(f\"  - {item['name']} ({item['sku']}) - {item['price']} {item['currency']} [{item['categoryName']}]\")
"
